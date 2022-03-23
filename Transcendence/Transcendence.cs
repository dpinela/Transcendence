using System.Collections;
using Modding;
using UnityEngine;
using USM = UnityEngine.SceneManagement;
using SFCore;
using ItemChanger;
using ItemChanger.Modules;
using ItemChanger.Locations;
using ItemChanger.Items;
using ItemChanger.UIDefs;
using RandomizerMod;

namespace Transcendence
{
    public class Transcendence : Mod, ILocalSettings<SaveSettings>
    {
        private static List<Charm> Charms = new() 
        {
            AntigravityAmulet.Instance,
            BluemothWings.Instance,
            LemmsStrength.Instance,
            ShinySlash.Instance,
            FloristsBlessing.Instance,
            SnailSoul.Instance,
            ShamanAmp.Instance,
            NitroCrystal.Instance,
            Crystalmaster.Instance,
            DisinfectantFlask.Instance,
            MillibellesBlessing.Instance,
            Greedsong.Instance,
            ChaosOrb.Instance
        };

        internal static Transcendence Instance;

        private Dictionary<string, Func<bool, bool>> BoolGetters = new();
        private Dictionary<string, Action<bool>> BoolSetters = new();
        private Dictionary<string, Func<int, int>> IntGetters = new();
        private Dictionary<(string, string), Action<PlayMakerFSM>> FSMEdits = new();
        private List<(int Period, Action Func)> Tickers = new();

        public override void Initialize()
        {
            Instance = this;
            foreach (var charm in Charms)
            {
                var num = CharmHelper.AddSprites(EmbeddedSprites.Get(charm.Sprite))[0];
                charm.Num = num;
                IntGetters[$"charmCost_{num}"] = _ => (Equipped(ChaosOrb.Instance) && ChaosOrb.Instance.GivingCharm(num)) ? 0 : charm.DefaultCost;
                AddTextEdit($"CHARM_NAME_{num}", "UI", charm.Name);
                AddTextEdit($"CHARM_DESC_{num}", "UI", () => charm.Description);
                var bools = charm.Settings;
                BoolGetters[$"equippedCharm_{num}"] = _ => bools(Settings).Equipped || (Equipped(ChaosOrb.Instance) && ChaosOrb.Instance.GivingCharm(num));
                BoolSetters[$"equippedCharm_{num}"] = value => bools(Settings).Equipped = value;
                BoolGetters[$"gotCharm_{num}"] = _ => bools(Settings).Got;
                BoolSetters[$"gotCharm_{num}"] = value => bools(Settings).Got = value;
                BoolGetters[$"newCharm_{num}"] = _ => bools(Settings).New;
                BoolSetters[$"newCharm_{num}"] = value => bools(Settings).New = value;
                charm.Hook();
                foreach (var edit in charm.FsmEdits)
                {
                    AddFsmEdit(edit.obj, edit.fsm, edit.edit);
                }
                Tickers.AddRange(charm.Tickers);
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
            ModHooks.LanguageGetHook += GetCharmStrings;
            // This will run after Rando has already set up its item placements.
            On.UIManager.StartNewGame += PlaceItems;
            On.PlayMakerFSM.OnEnable += EditFSMs;
            USM.SceneManager.activeSceneChanged += StartTicking;
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

        public override string GetVersion() => "1.0";

        private SaveSettings Settings = new();

        public void OnLoadLocal(SaveSettings s)
        {
            Settings = s;
        }

        public SaveSettings OnSaveLocal() => Settings;

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

        private void StartTicking(USM.Scene from, USM.Scene to)
        {
            // The coroutines get stopped every time the player quits out, so we
            // need to restart them. This is a good point to do so.
            if (from.name == "Menu_Title")
            {
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

                    GameManager.instance.StartCoroutine(ticker());
                }
            }
        }

        private static void PlaceItems(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            ItemChangerMod.CreateSettingsProfile(overwrite: false);

            var placements = new List<AbstractPlacement>();

            foreach (var charm in Charms)
            {
                var name = charm.Name.Replace(" ", "_");
                placements.Add(
                    new CoordinateLocation() { x = charm.X, y = charm.Y, elevation = 0, sceneName = charm.Scene, name = name }
                    .Wrap()
                    .Add(new ItemChanger.Items.CharmItem() { charmNum = charm.Num, name = name, UIDef =
                        new MsgUIDef() { 
                            name = new LanguageString("UI", $"CHARM_NAME_{charm.Num}"),
                            shopDesc = new LanguageString("UI", $"CHARM_DESC_{charm.Num}"),
                            sprite = new EmbeddedSprite() { key = charm.Sprite }
                        }}));
            }

            ItemChangerMod.AddPlacements(placements, conflictResolution: PlacementConflictResolution.Ignore);

            orig(self, permaDeath, bossRush);
        }

        private static bool IsRandoActive() =>
            ModHooks.GetMod("Randomizer 4") != null && RandomizerMod.RandomizerMod.RS?.GenerationSettings != null;

        private static void ConfigureICModules()
        {
            if (!IsRandoActive())
            {
                ItemChangerMod.Modules.Remove<AutoUnlockIselda>();
                ItemChangerMod.Modules.Remove<BaldurHealthCap>();
                ItemChangerMod.Modules.Remove<CliffsShadeSkipAssist>();
                ItemChangerMod.Modules.Remove<DreamNailCutsceneEvent>();
                ItemChangerMod.Modules.Remove<FastGrubfather>();
                ItemChangerMod.Modules.Remove<GreatHopperEasterEgg>();
                ItemChangerMod.Modules.Remove<InventoryTracker>();
                ItemChangerMod.Modules.Remove<MenderbugUnlock>();
                ItemChangerMod.Modules.Remove<NonlinearColosseums>();
                ItemChangerMod.Modules.Remove<PreventLegEaterDeath>();
                ItemChangerMod.Modules.Remove<PreventZoteDeath>();
                ItemChangerMod.Modules.Remove<RemoveVoidHeartEffects>();
                ItemChangerMod.Modules.Remove<ReusableBeastsDenEntrance>();
                ItemChangerMod.Modules.Remove<ReusableCityCrestGate>();
                ItemChangerMod.Modules.Remove<ReverseBeastDenPath>();
                ItemChangerMod.Modules.Remove<RightCityPlatform>();
            }
        }

        internal static void UpdateNailDamage()
        {
            IEnumerator WaitThenUpdate()
            {
                yield return null;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            }
            GameManager.instance.StartCoroutine(WaitThenUpdate());
        }
    }
}
