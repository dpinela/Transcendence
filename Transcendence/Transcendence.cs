using System;
using System.IO;
using System.Collections;
using Modding;
using UnityEngine;
using SFCore;
using ItemChanger;
using ItemChanger.Modules;
using ItemChanger.Locations;
using ItemChanger.Items;
using ItemChanger.Tags;
using ItemChanger.Placements;
using ItemChanger.UIDefs;
using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod;
using RandomizerMod.Menu;
using RandomizerMod.Settings;
using RandomizerMod.Logging;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;

namespace Transcendence
{
    public class Transcendence : Mod, ILocalSettings<SaveSettings>, IGlobalSettings<GlobalSettings>, IMenuMod
    {
        internal static List<Charm> Charms = new()
        {
            AntigravityAmulet.Instance,
            BluemothWings.Instance,
            LemmsStrength.Instance,
            FloristsBlessing.Instance,
            // needs to hook after the previous two so that the player can't negate
            // the drawback of Snail Slash with them
            SnailSlash.Instance,
            SnailSoul.Instance,
            ShamanAmp.Instance,
            NitroCrystal.Instance,
            Crystalmaster.Instance,
            DisinfectantFlask.Instance,
            MillibellesBlessing.Instance,
            Greedsong.Instance,
            MarissasAudience.Instance,
            ChaosOrb.Instance
        };

        internal static Transcendence Instance;

        private Dictionary<string, Func<bool, bool>> BoolGetters = new();
        private Dictionary<string, Action<bool>> BoolSetters = new();
        private Dictionary<string, Func<int, int>> IntGetters = new();
        private Dictionary<string, Func<int, int>> IntSetters = new();
        private Dictionary<(string, string), Action<PlayMakerFSM>> FSMEdits = new();
        private List<(int Period, Action Func)> Tickers = new();

        public override void Initialize()
        {
            Log("Initializing");
            Instance = this;
            foreach (var charm in Charms)
            {
                var num = CharmHelper.AddSprites(EmbeddedSprites.Get(charm.Sprite))[0];
                charm.Num = num;
                var settings = charm.Settings;
                IntGetters[$"charmCost_{num}"] = _ => (Equipped(ChaosOrb.Instance) && ChaosOrb.Instance.GivingCharm(num)) ? 0 : settings(Settings).Cost;
                IntSetters[$"charmCost_{num}"] = value => settings(Settings).Cost = value;
                AddTextEdit($"CHARM_NAME_{num}", "UI", charm.Name);
                AddTextEdit($"CHARM_DESC_{num}", "UI", () => charm.Description);
                BoolGetters[$"equippedCharm_{num}"] = _ => settings(Settings).Equipped || (Equipped(ChaosOrb.Instance) && ChaosOrb.Instance.GivingCharm(num));
                BoolSetters[$"equippedCharm_{num}"] = charm == ChaosOrb.Instance ?
                    (value => settings(Settings).Equipped = value || Settings.ChaosMode)
                     : (value => settings(Settings).Equipped = value);
                BoolGetters[$"gotCharm_{num}"] = _ => settings(Settings).Got;
                BoolSetters[$"gotCharm_{num}"] = value => settings(Settings).Got = value;
                BoolGetters[$"newCharm_{num}"] = _ => settings(Settings).New;
                BoolSetters[$"newCharm_{num}"] = value => settings(Settings).New = value;
                charm.Hook();
                foreach (var edit in charm.FsmEdits)
                {
                    AddFsmEdit(edit.obj, edit.fsm, edit.edit);
                }
                Tickers.AddRange(charm.Tickers);

                var item = new ItemChanger.Items.CharmItem() { 
                    charmNum = charm.Num,
                    name = charm.Name.Replace(" ", "_"),
                    UIDef = new MsgUIDef() { 
                        name = new LanguageString("UI", $"CHARM_NAME_{charm.Num}"),
                        shopDesc = new LanguageString("UI", $"CHARM_DESC_{charm.Num}"),
                        sprite = new EmbeddedSprite() { key = charm.Sprite }
                    }};
                // Tag the item for ConnectionMetadataInjector, so that MapModS and
                // other mods recognize the items we're adding as charms.
                var mapmodTag = item.AddTag<InteropTag>();
                mapmodTag.Message = "RandoSupplementalMetadata";
                mapmodTag.Properties["ModSource"] = GetName();
                mapmodTag.Properties["PoolGroup"] = "Charms";
                Finder.DefineCustomItem(item);
            }
            for (var i = 1; i <= 40; i++)
            {
                var num = i; // needed for closure to capture a different copy of the variable each time
                BoolGetters[$"equippedCharm_{num}"] = value => value || (Equipped(ChaosOrb.Instance) && ChaosOrb.Instance.GivingCharm(num));
                IntGetters[$"charmCost_{num}"] = value => (Equipped(ChaosOrb.Instance) && ChaosOrb.Instance.GivingCharm(num)) ? 0 : value;
            }

            ModHooks.GetPlayerBoolHook += ReadCharmBools;
            ModHooks.SetPlayerBoolHook += WriteCharmBools;
            ModHooks.GetPlayerIntHook += ReadCharmCosts;
            // This exists so that we can store charm costs in PlayerDataEditModule for the benefit
            // of ItemChangerDataLoader users.
            ModHooks.SetPlayerIntHook += WriteCharmCosts;
            ModHooks.LanguageGetHook += GetCharmStrings;
            // This will run after Rando has already set up its item placements.
            On.UIManager.StartNewGame += PlaceItems;
            On.PlayMakerFSM.OnEnable += EditFSMs;
            // This hook is set before ItemChanger's, so AutoSalubraNotches will take our charms into account.
            On.PlayerData.CountCharms += CountOurCharms;
            On.PlayerData.UnequipCharm += BlockChaosOrbUnequip;
            StartTicking();

            if (ModHooks.GetMod("Randomizer 4") != null)
            {
                // The code that references rando needs to be in a separate method
                // so that the mod will still load without it installed
                // (trying to run a method whose code references an unavailable
                // DLL will fail even if the code in question isn't actually run)
                HookRando();
            }
            if (ModHooks.GetMod("DebugMod") != null)
            {
                DebugModHook.GiveAllCharms(() => {
                    GrantAllOurCharms();
                    PlayerData.instance.CountCharms();
                });
            }
        }

        // breaks infinite loop when reading equippedCharm_X
        private bool Equipped(Charm c) => c.Settings(Settings).Equipped;

        private Dictionary<(string Key, string Sheet), Func<string>> TextEdits = new();

        internal void AddTextEdit(string key, string sheetName, string text)
        {
            TextEdits.Add((key, sheetName), () => text);
        }

        internal void AddTextEdit(string key, string sheetName, Func<string> text)
        {
            TextEdits.Add((key, sheetName), text);
        }

        public override string GetVersion() => "1.2.2";

        internal SaveSettings Settings = new();

        public void OnLoadLocal(SaveSettings s)
        {
            Settings = s;
            FloristsBlessing.Instance.Broken = s.FloristsBlessingBroken;
            ChaosOrb.Instance.GivenCharms = s.ChaosOrbGivenCharms;
        }

        public SaveSettings OnSaveLocal()
        {
            Settings.FloristsBlessingBroken = FloristsBlessing.Instance.Broken;
            Settings.ChaosOrbGivenCharms = ChaosOrb.Instance.GivenCharms;
            return Settings;
        }

        internal GlobalSettings ModSettings = new();

        public void OnLoadGlobal(GlobalSettings s)
        {
            ModSettings = s;
            RandoSettings = new(s);
            ChaosOrb.Instance.InitChaosHudSettings(s.ChaosHud);
        }

        public GlobalSettings OnSaveGlobal()
        {
            ModSettings.AddCharms = RandoSettings.AddCharms;
            ModSettings.IncreaseMaxCharmCostBy = RandoSettings.IncreaseMaxCharmCostBy;
            return ModSettings;
        }

        private bool ReadCharmBools(string boolName, bool value)
        {
            if (BoolGetters.TryGetValue(boolName, out var f))
            {
                return f(value);
            }
            return value;
        }

        private bool WriteCharmBools(string boolName, bool value)
        {
            if (BoolSetters.TryGetValue(boolName, out var f))
            {
                f(value);
            }
            return value;
        }

        private int ReadCharmCosts(string intName, int value)
        {
            if (IntGetters.TryGetValue(intName, out var cost))
            {
                return cost(value);
            }
            return value;
        }

        private int WriteCharmCosts(string intName, int value)
        {
            if (IntSetters.TryGetValue(intName, out var cost))
            {
                return cost(value);
            }
            return value;
        }

        private string GetCharmStrings(string key, string sheetName, string orig)
        {
            if (TextEdits.TryGetValue((key, sheetName), out var text))
            {
                return text();
            }
            return orig;
        }

        internal void AddFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> edit)
        {
            var key = (objName, fsmName);
            var newEdit = edit;
            if (FSMEdits.TryGetValue(key, out var orig))
            {
                newEdit = fsm => {
                    orig(fsm);
                    edit(fsm);
                };
            }
            FSMEdits[key] = newEdit;
        }

        private void EditFSMs(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM fsm)
        {
            orig(fsm);
            if (FSMEdits.TryGetValue((fsm.gameObject.name, fsm.FsmName), out var edit))
            {
                edit(fsm);
            }
        }

        private void StartTicking()
        {
            // Use our own object to hold timers so that GameManager.StopAllCoroutines
            // does not kill them.
            var timerHolder = new GameObject("Timer Holder");
            GameObject.DontDestroyOnLoad(timerHolder);
            var timers = timerHolder.AddComponent<DummyMonoBehaviour>();
            foreach (var t in Tickers)
            {
                IEnumerator ticker()
                {
                    while (true)
                    {
                        try
                        {
                            t.Func();
                        }
                        catch (Exception ex)
                        {
                            LogError(ex);
                        }
                        yield return new WaitForSeconds(t.Period);
                    }
                }

                timers.StartCoroutine(ticker());
            }
        }

        private void CountOurCharms(On.PlayerData.orig_CountCharms orig, PlayerData self)
        {
            orig(self);
            self.SetInt("charmsOwned", self.GetInt("charmsOwned") + Charms.Count(c => c.Settings(Settings).Got));
        }

        private void PlaceItems(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            ItemChangerMod.CreateSettingsProfile(overwrite: false, createDefaultModules: false);
            if (ModHooks.GetMod("Randomizer 4") != null && IsRandoActive())
            {
                PlaceItemsRando();
                
            }
            else
            {
                ConfigureICModules();
                PlaceCharmsAtFixedPositions();
                PlaceFloristsBlessingRepair();
                StoreNotchCosts(DefaultNotchCosts());
            }
            // Even in rando, we want to add the starting Chaos Orb directly rather
            // than going through the RequestBuilder because doing it that way would
            // cause placements to change.
            if (ModSettings.ChaosMode)
            {
                SetupChaosMode();
            }

            if (bossRush)
            {
                GrantAllOurCharms();
                GrantGodhomeStartingItems();
            }
            
            orig(self, permaDeath, bossRush);
        }

        private void PlaceItemsRando()
        {
            var gs = RandomizerMod.RandomizerMod.RS.GenerationSettings;
            var costs = gs.MiscSettings.RandomizeNotchCosts ? RandomizeNotchCosts(gs.Seed) : DefaultNotchCosts();
            
            StoreNotchCosts(costs);

            if (gs.PoolSettings.Charms)
            {
                if (RandoSettings.AddCharms)
                {
                    PlaceFloristsBlessingRepair();
                }
            }
            else
            {
                PlaceCharmsAtFixedPositions();
                PlaceFloristsBlessingRepair();
            }
        }

        private void GrantGodhomeStartingItems()
        {
            PlayerData.instance.SetInt("geo", 50000);
            PlayerData.instance.SetInt("trinket1", 14);
            PlayerData.instance.SetInt("trinket2", 17);
            PlayerData.instance.SetInt("trinket3", 8);
            PlayerData.instance.SetInt("trinket4", 4);
            PlayerData.instance.SetBool("foundTrinket1", true);
            PlayerData.instance.SetBool("foundTrinket2", true);
            PlayerData.instance.SetBool("foundTrinket3", true);
            PlayerData.instance.SetBool("foundTrinket4", true);
        }

        private static void PlaceCharmsAtFixedPositions()
        {
            var placements = new List<AbstractPlacement>();
            foreach (var charm in Charms)
            {
                var name = charm.Name.Replace(" ", "_");
                placements.Add(
                    new CoordinateLocation() { x = charm.X, y = charm.Y, elevation = 0, sceneName = charm.Scene, name = name }
                    .Wrap()
                    .Add(Finder.GetItem(name)));
            }
            ItemChangerMod.AddPlacements(placements, conflictResolution: PlacementConflictResolution.Ignore);
        }

        private void SetupChaosMode()
        {
            // Use MergeKeepingOld so that we don't conflict with any starting items
            // that rando gives.
            ItemChangerMod.AddPlacements(new List<AbstractPlacement>()
            {
                Finder.GetLocation("Start").Wrap().Add(Finder.GetItem(ChaosOrb.Instance.InternalName)),
            }, conflictResolution: PlacementConflictResolution.MergeKeepingOld);

            // store the Chaos Mode setting and initialization routine for later use with ICDL
            ItemChangerMod.Modules.Add<ChaosModeModule>();
        }

        private void BlockChaosOrbUnequip(On.PlayerData.orig_UnequipCharm orig, PlayerData pd, int charmNum)
        {
            if (!(charmNum == ChaosOrb.Instance.Num && Settings.ChaosMode))
            {
                orig(pd, charmNum);
            }
        }

        private static void PlaceFloristsBlessingRepair()
        {
            ItemChangerMod.AddPlacements(new List<AbstractPlacement>()
            {
                MakeFloristsBlessingPlacement("Florist's_Blessing_Repair", "RestingGrounds_12", 72.0f, 3.4f),
                MakeFloristsBlessingPlacement("Florist's_Blessing_Repair_Godhome", "GG_Atrium", 155.6f, 61.4f)
            }, conflictResolution: PlacementConflictResolution.Ignore);
        }

        private static MutablePlacement MakeFloristsBlessingPlacement(string name, string scene, float x, float y)
        {
            var repairPlacement = new CoordinateLocation() { x = x, y = y, elevation = 0, sceneName = scene, name = name }.Wrap() as MutablePlacement;
            repairPlacement.Cost = new RecurringGeoCost(FloristsBlessing.RepairCost);
            repairPlacement.Add(new FloristsBlessingRepairItem());
            return repairPlacement;
        }

        private const int MinTotalCost = 22;
        private const int MaxTotalCost = 35;

        private Dictionary<int, int> RandomizeNotchCosts(int seed)
        {
            // This log statement is here to help diagnose a possible bug where charms cost more than
            // they ever should.
            var rng = new System.Random(seed);
            var total = rng.Next(MinTotalCost, MaxTotalCost + 1);
            Log($"Randomizing notch costs; total cost = {total}");
            var costs = Charms.ToDictionary(c => c.Num, c => 0);
            for (var i = 0; i < total; i++)
            {
                var possiblePicks = costs.Where(c => c.Value < 6).Select(c => c.Key).ToList();
                if (possiblePicks.Count == 0)
                {
                    break;
                }
                var pick = rng.Next(possiblePicks.Count);
                costs[possiblePicks[pick]]++;
            }
            // ChaosModeModule will set the cost in this case, avoid setting it twice to avoid
            // order-dependency.
            if (ModSettings.ChaosMode)
            {
                costs.Remove(ChaosOrb.Instance.Num);
            }
            return costs;
        }

        private Dictionary<int, int> DefaultNotchCosts() {
            var costs = Charms.ToDictionary(c => c.Num, c => c.DefaultCost);
            if (ModSettings.ChaosMode)
            {
                costs.Remove(ChaosOrb.Instance.Num);
            }
            return costs;
        }

        // Store notch costs in an ItemChanger module so that ICDL will reload them.
        private void StoreNotchCosts(Dictionary<int, int> costs)
        {
            var icPlayerData = ItemChangerMod.Modules.GetOrAdd<ItemChanger.Modules.PlayerDataEditModule>();
            foreach ((var num, var cost) in costs)
            {
                icPlayerData.AddPDEdit($"charmCost_{num}", cost);
            }
        }

        private void HookRando()
        {
            RequestBuilder.OnUpdate.Subscribe(-498, DefineCharmsForRando);
            RequestBuilder.OnUpdate.Subscribe(-200, IncreaseMaxCharmCost);
            RequestBuilder.OnUpdate.Subscribe(50, AddCharmsToPool);
            RCData.RuntimeLogicOverride.Subscribe(50, DefineLogicItems);
            RandomizerMenuAPI.AddMenuPage(BuildMenu, BuildButton);
            SettingsLog.AfterLogSettings += LogRandoSettings;
        }

        // This is actually a MenuPage, but we can't use that as the static type because then this mod won't
        // load without MenuChanger installed because the runtime can't load the type of the field.
        private object SettingsPage;
        private RandoSettings RandoSettings = new(new GlobalSettings());

        private void BuildMenu(MenuPage landingPage)
        {
            var sp = new MenuPage(GetName(), landingPage);
            SettingsPage = sp;
            var factory = new MenuElementFactory<RandoSettings>(sp, RandoSettings);
            new VerticalItemPanel(sp, new(0, 300), 75f, true, factory.Elements);
        }

        private bool BuildButton(MenuPage landingPage, out SmallButton settingsButton)
        {
            settingsButton = new(landingPage, GetName());
            settingsButton.AddHideAndShowEvent(landingPage, (MenuPage)SettingsPage);
            return true;
        }

        private void LogRandoSettings(LogArguments args, TextWriter w)
        {
            w.WriteLine("Logging Transcendence settings:");
            w.WriteLine(JsonUtil.Serialize(RandoSettings));
        }

        public bool ToggleButtonInsideMenu => false;

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggle) => new()
        {
            new()
            {
                Name = "Chaos Mode",
                Description = "Start with 0-cost Chaos Orb permanently equipped.",
                Values = new[] { "Off", "On" },
                Saver = i => { ModSettings.ChaosMode = i == 1; },
                Loader = () => ModSettings.ChaosMode ? 1 : 0
            },
            new()
            {
                Name = "Chaos HUD",
                Description = "Display charms currently given by Chaos Orb on screen.",
                Values = new[] { "Off", "On" },
                Saver = i => {
                    ModSettings.ChaosHud.Enabled = i == 1;
                    ChaosOrb.Instance.UpdateChaosHudSettings(ModSettings.ChaosHud);
                },
                Loader = () => ModSettings.ChaosHud.Enabled ? 1 : 0
            },
            new()
            {
                Name = "Chaos HUD Horiz. Position",
                Values = new[] { "Left", "Center", "Right" },
                Saver = i => {
                    ModSettings.ChaosHud.HorizontalPosition = i;
                    ChaosOrb.Instance.UpdateChaosHudSettings(ModSettings.ChaosHud);
                },
                Loader = () => ModSettings.ChaosHud.HorizontalPosition
            },
            new()
            {
                Name = "Chaos HUD Vertical Position",
                Values = new[] { "Top", "Center", "Bottom" },
                Saver = i => {
                    ModSettings.ChaosHud.VerticalPosition = i;
                    ChaosOrb.Instance.UpdateChaosHudSettings(ModSettings.ChaosHud);
                },
                Loader = () => ModSettings.ChaosHud.VerticalPosition
            },
            new()
            {
                Name = "Chaos HUD Orientation",
                Values = new[] { "Horizontal", "Vertical" },
                Saver = i => {
                    ModSettings.ChaosHud.Orientation = i;
                    ChaosOrb.Instance.UpdateChaosHudSettings(ModSettings.ChaosHud);
                },
                Loader = () => ModSettings.ChaosHud.Orientation
            }
        };

        private static void DefineCharmsForRando(RequestBuilder rb)
        {
            if (!rb.gs.PoolSettings.Charms)
            {
                return;
            }
            var names = new HashSet<string>();
            foreach (var charm in Charms)
            {
                var name = charm.Name.Replace(" ", "_");
                names.Add(name);
                rb.EditItemRequest(name, info =>
                {
                    info.getItemDef = () => new()
                    {
                        Name = name,
                        Pool = "Charm",
                        MajorItem = false,
                        PriceCap = 666
                    };
                });
            }

            rb.OnGetGroupFor.Subscribe(0f, (RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb) => {
                if (names.Contains(item) && (type == RequestBuilder.ElementType.Unknown || type == RequestBuilder.ElementType.Item))
                {
                    gb = rb.GetGroupFor("Shaman_Stone");
                    return true;
                }
                gb = default;
                return false;
            });
        }

        private void IncreaseMaxCharmCost(RequestBuilder rb)
        {
            // This limitation could be lifted 
            if (rb.gs.PoolSettings.Charms && RandoSettings.AddCharms)
            {
                rb.gs.CostSettings.MaximumCharmCost += RandoSettings.IncreaseMaxCharmCostBy;
            }
        }

        private static void DefineLogicItems(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!gs.PoolSettings.Charms)
            {
                return;
            }
            foreach (var charm in Charms)
            {
                var name = charm.Name.Replace(" ", "_");
                var term = lmb.GetOrAddTerm(name);
                var oneOf = new TermValue(term, 1);
                lmb.AddItem(new CappedItem(name, new TermValue[]
                {
                    oneOf,
                    new TermValue(lmb.GetTerm("CHARMS"), 1)
                }, oneOf));
            }
        }

        private void AddCharmsToPool(RequestBuilder rb)
        {
            if (!(rb.gs.PoolSettings.Charms && RandoSettings.AddCharms))
            {
                return;
            }
            foreach (var charm in Charms)
            {
                rb.AddItemByName(charm.Name.Replace(" ", "_"));
            }
        }

        private static bool IsRandoActive() =>
            RandomizerMod.RandomizerMod.RS?.GenerationSettings != null;

        private static void ConfigureICModules()
        {
            // Just to add the hook that Chaos Orb uses to turn on Fury.
            ItemChangerMod.Modules.GetOrAdd<FixFury>();
            ItemChangerMod.Modules.GetOrAdd<LeftCityChandelier>();
            ItemChangerMod.Modules.GetOrAdd<PlayerDataEditModule>();
            ItemChangerMod.Modules.GetOrAdd<RespawnCollectorJars>();
            ItemChangerMod.Modules.GetOrAdd<TransitionFixes>();
        }

        internal static void UpdateNailDamage()
        {
            DoNextFrame(() => PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE"));
        }

        internal static void DoNextFrame(Action f)
        {
            IEnumerator WaitThenCall()
            {
                yield return null;
                f();
            }
            GameManager.instance.StartCoroutine(WaitThenCall());
        }

        private void GrantAllOurCharms()
        {
            foreach (var charm in Charms)
            {
                charm.Settings(Settings).Got = true;
            }
        }
    }
}
