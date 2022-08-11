using System;
using System.Collections;
using Modding;
using ItemChanger;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using USM = UnityEngine.SceneManagement;

namespace Transcendence
{
    internal class ChaosOrb : Charm
    {
        public static readonly ChaosOrb Instance = new();

        private ChaosOrb() {}

        public override string Sprite => "ChaosOrb.png";
        public override string Name => "Chaos Orb";
        public override string Description => $"Formed in the depths of the Abyss from fragments of discarded charms.\n\nThe bearer will gain the effects of three random charms, changing over time.\n\nCurrently granting the effects of {GivenCharmDescription()}.";
        public override int DefaultCost => 2;
        public override string Scene => "Deepnest_East_04";
        public override float X => 27.5f;
        public override float Y => 80.4f;

        public string InternalName => Name.Replace(" ", "_");

        public override CharmSettings Settings(SaveSettings s) => s.ChaosOrb;

        private const int TickPeriod = 30;

        public override List<(int, Action)> Tickers => new() {(TickPeriod, RerollCharmsIfEquipped)};

        public override void Hook()
        {
            ModHooks.SetPlayerBoolHook += RerollCharmsAndOtherStuffOnEquip;
            // Does not apply to debug upgrades/downgrades since it doesn't use SetInt to do them.
            ModHooks.SetPlayerIntHook += UpdateChaosHudOnKingsoulUpgrade;
            ModHooks.GetPlayerIntHook += EnableKingsoul;
            ModHooks.GetPlayerBoolHook += EnableUnbreakableCharms;
            // Temporarily stop giving any charms while ItemChanger is giving an item. This solves two issues:
            // - While picking up a WhiteFragmentItem, having royalCharmState raised to 3 from EnableKingsoul would cause
            // the fragment to erroneously turn into Void Heart.
            // - While picking up a Fragile/Unbreakable charm, having it set to unbreakable from EnableUnbreakableCharms
            // would cause... problems. (Probably, the Fragile version would be given as normal, but the Unbreakable
            // pickup would be treated as a duplicate as the game thinks you already have it.)
            // - When NotchCostUI is active, picking up a charm that is currently being granted by the Orb would make
            // the pickup message display the cost as 0 instead of the charm's actual cost.
            AbstractItem.ModifyItemGlobal += DisableWhileGivingItem;

            On.HeroController.Awake += SetupChaosHud;
            // Can't use ItemChanger's scene change hook because it doesn't trigger when exiting to
            // Menu_Title.
            USM.SceneManager.activeSceneChanged += ToggleChaosHud;
            GrabRoyalCharmIcons();
        }

        public List<int> GivenCharms = new();
        public bool GivingCharm(int num) => GivenCharms.Contains(num);

        private string GivenCharmDescription() =>
            GivenCharms.Count switch {
                0 => "nothing",
                1 => CharmName(GivenCharms[0]),
                _ => String.Join(", ", GivenCharms.GetRange(0, GivenCharms.Count - 1).Select(CharmName)) + " and " + CharmName(GivenCharms[GivenCharms.Count - 1])
            };

        private static string CharmName(int num) {
            var key = num switch {
                36 => PlayerData.instance.GetInt("royalCharmState") > 3 ? "CHARM_NAME_36_C" : "CHARM_NAME_36_B",
                23 or 24 or 25 => $"CHARM_NAME_{num}_G",
                _ => $"CHARM_NAME_{num}"
            };
            return Language.Language.Get(key, "UI");
        }

        internal event Action<List<int>, List<int>> OnReroll;

        private System.Random rng = new();

        private List<int> PickNUnequippedCharms(int n)
        {
            var unequippedCharms = new List<int>();
            for (var i = 1; i <= 40; i++)
            {
                // Joni's Blessing is excluded because it causes wonky behaviour
                // when given by this charm.
                if (!(i == 27 || PlayerData.instance.GetBool($"equippedCharm_{i}")))
                {
                    unequippedCharms.Add(i);
                }
            }
            foreach (var charm in Transcendence.Charms)
            {
                if (charm != this && !PlayerData.instance.GetBool($"equippedCharm_{charm.Num}"))
                {
                    unequippedCharms.Add(charm.Num);
                }
            }
            if (n > unequippedCharms.Count)
            {
                return unequippedCharms;
            }
            for (var i = 0; i < n; i++)
            {
                var pick = rng.Next(unequippedCharms.Count - i);
                var c = unequippedCharms[i + pick];
                unequippedCharms[i + pick] = unequippedCharms[i];
                unequippedCharms[i] = c;
            }
            return unequippedCharms.GetRange(0, n);
        }

        public void RerollCharms()
        {
            var oldCharms = GivenCharms;
            GivenCharms = empty; // so that charms currently given by the Orb can be selected again
            GivenCharms = PickNUnequippedCharms(3);
            OnReroll?.Invoke(oldCharms, GivenCharms);
            UpdateHud();

            PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
            PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            if (GivenCharms.Contains(6))
            {
                if (Health() == 1)
                {
                    PlayMakerFSM.BroadcastEvent("ENABLE FURY");
                }
                
            }
            // what if the player equipped it manually while Orb was giving it?
            else if (!PlayerData.instance.GetBool("equippedCharm_6"))
            {
                PlayMakerFSM.BroadcastEvent("DISABLE FURY");
            }
        }

        private static int Health() => 
            PlayerData.instance.GetInt(PlayerData.instance.GetBool("equippedCharm_27") ? "joniHealthBlue" : "health");

        private void RerollCharmsIfEquipped()
        {
            if (Equipped() && HeroController.instance != null)
            {
                RerollCharms();
            }
        }

        private bool RerollCharmsAndOtherStuffOnEquip(string boolName, bool value)
        {
            if (boolName == $"equippedCharm_{Num}")
            {
                if (value)
                {
                    // Technically it might be the case that the last tick
                    // actually picked zero charms, but in that case every
                    // charm is equipped, so rerolling will make no difference.
                    if (GivenCharms.Count == 0)
                    {
                        RerollCharms();
                    }
                    if (HudSlots != null)
                    {
                        HudSlots.Visibility = Visibility.Visible;
                        UpdateHud();
                    }
                }
                else if (HudSlots != null)
                {
                    HudSlots.Visibility = Visibility.Collapsed;
                }
                RerollCharms();
            }
            return value;
        }

        private static bool GivingVanillaWhiteCharm() =>
            GameObject.Find("UI Msg Get WhiteCharm(Clone)")?.LocateMyFSM("Msg Control")?.Fsm.ActiveStateName == "Init";

        private int EnableKingsoul(string intName, int value)
        {
            // We cannot pretend to have Kingsoul while vanilla White Fragment/Kingsoul/Void Heart
            // animations are running, or the game will softlock AND, at least in some cases,
            // lock the player out of the item they were supposed to get.
            if (intName == "royalCharmState" && value < 3 && GivingCharm(36) && Equipped() && !GivingVanillaWhiteCharm())
            {
                value = 3;
            }
            return value;
        }

        private bool EnableUnbreakableCharms(string boolName, bool value) =>
            boolName switch {
                nameof(PlayerData.fragileHealth_unbreakable) => value || (Equipped() && GivingCharm(23)),
                nameof(PlayerData.fragileGreed_unbreakable) => value || (Equipped() && GivingCharm(24)),
                nameof(PlayerData.fragileStrength_unbreakable) => value || (Equipped() && GivingCharm(25)),
                
                nameof(PlayerData.brokenCharm_23) => value && !(Equipped() && GivingCharm(23)),
                nameof(PlayerData.brokenCharm_24) => value && !(Equipped() && GivingCharm(24)),
                nameof(PlayerData.brokenCharm_25) => value && !(Equipped() && GivingCharm(25)),
                _ => value
            };

        private static readonly List<int> empty = new();

        private void DisableWhileGivingItem(GiveEventArgs args)
        {
            // args.Info may be null in contexts where no item is being given.
            if (args == null || args.Info == null)
            {
                return;
            }
            var givenCharms = GivenCharms;
            GivenCharms = empty;
            args.Info.Callback += (AbstractItem it) => {
                GivenCharms = givenCharms;
            };
        }

        private StackLayout HudSlots;
        private Sprite KingsoulIcon;
        private Sprite VoidHeartIcon;

        private ChaosHudSettings HudSettings = new();
        private const float IconSize = 75f;

        internal void UpdateHud()
        {
            if (HudSlots == null || HudSlots.Visibility != Visibility.Visible)
            {
                return;
            }
            // CharmIconList isn't loaded yet the first time the game loads into a gameplay scene.
            // In that case, wait until it is.
            if (CharmIconList.Instance == null || HeroController.instance == null)
            {
                IEnumerator UpdateLater()
                {
                    yield return new WaitWhile(() => HeroController.instance == null || CharmIconList.Instance == null);
                    UpdateHud();
                }

                GameManager.instance.StartCoroutine(UpdateLater());
                return;
            }

            Transcendence.Instance.Log("Updating Chaos HUD");

            var slots = HudSlots.Children;
            slots.Clear();
            foreach (var charmNum in GivenCharms)
            {
                var sprite = CharmSprite(charmNum);
                var img = new Image(HudSlots.LayoutRoot, sprite, "Chaos HUD Item");
                img.Width = IconSize;
                img.Height = IconSize;
                slots.Add(img);
            }
        }

        private Sprite CharmSprite(int num) => num switch {
            36 => PlayerData.instance.GetInt("royalCharmState") > 3 ? VoidHeartIcon : KingsoulIcon,
            <= 40 => CharmIconList.Instance.GetSprite(num),
            _ => EmbeddedSprites.Get(Transcendence.Charms.First(c => c.Num == num).Sprite)
        };

        private static HorizontalAlignment HorizAlignmentFromIndex(int n) => n switch
        {
            0 => HorizontalAlignment.Left,
            1 => HorizontalAlignment.Center,
            _ => HorizontalAlignment.Right
        };

        private static VerticalAlignment VertAlignmentFromIndex(int n) => n switch
        {
            1 => VerticalAlignment.Center,
            2 => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Top,
        };

        private static Orientation OrientationFromIndex(int n) => n switch
        {
            0 => Orientation.Horizontal,
            _ => Orientation.Vertical
        };

        internal void InitChaosHudSettings(ChaosHudSettings s)
        {
            HudSettings = s;
        }

        internal void UpdateChaosHudSettings(ChaosHudSettings s)
        {
            if (HudSlots != null)
            {
                if (s.Enabled && !HudSettings.Enabled)
                {
                    if (GameManager.instance.IsGameplayScene() && Equipped())
                    {
                        HudSlots.Visibility = Visibility.Visible;
                        UpdateHud();
                    }
                }
                else if (!s.Enabled && HudSettings.Enabled)
                {
                    HudSlots.Visibility = Visibility.Collapsed;
                }
                ConfigureChaosHud(s);
            }
            HudSettings = s;
        }

        private void ConfigureChaosHud(ChaosHudSettings s)
        {
            HudSlots.HorizontalAlignment = HorizAlignmentFromIndex(s.HorizontalPosition);
            HudSlots.VerticalAlignment = VertAlignmentFromIndex(s.VerticalPosition);
            HudSlots.Orientation = OrientationFromIndex(s.Orientation);
            HudSlots.Spacing = s.Spacing;
        }

        private void SetupChaosHud(On.HeroController.orig_Awake orig, HeroController self)
        {
            Transcendence.Instance.Log("Initializing Chaos HUD");
            var root = new LayoutRoot(true, "Chaos HUD");
            HudSlots = new StackLayout(root, "Given Charms");
            ConfigureChaosHud(HudSettings);
            HudSlots.Visibility = Visibility.Collapsed;

            orig(self);
        }

        private void ToggleChaosHud(USM.Scene from, USM.Scene to)
        {
            if (HudSlots == null)
            {
                return;
            }
            if (GameManager.instance.IsGameplayScene())
            {
                if (HudSettings.Enabled && HudSlots.Visibility != Visibility.Visible && Equipped())
                {
                    Transcendence.Instance.Log("Turning on Chaos HUD");
                    HudSlots.Visibility = Visibility.Visible;
                    UpdateHud();
                }
            }
            else
            {
                Transcendence.Instance.Log("Turning off Chaos HUD");
                HudSlots.Visibility = Visibility.Collapsed;
            }
        }

        private int UpdateChaosHudOnKingsoulUpgrade(string intName, int value)
        {
            if (intName == "royalCharmState" && HudSlots?.Visibility == Visibility.Visible)
            {
                // We cannot just wait until the change to PlayerData is written, because the
                // big item UI for the new Kingsoul level (if given by ItemChanger) may still be
                // up, and during that time the Orb is disabled by DisableWhileGivingItem.
                IEnumerator UpdateHudAfterItemGiven()
                {
                    yield return null;
                    yield return new WaitWhile(() => GivenCharms.Count == 0);
                    UpdateHud();
                }
                GameManager.instance.StartCoroutine(UpdateHudAfterItemGiven());
            }
            return value;
        }

        // CharmIconList does not have the sprites for Kingsoul and Void Heart for some reason; in their
        // place it has the Dashmaster-esque unused charm icon.
        private void GrabRoyalCharmIcons()
        {
            KingsoulIcon = Finder.GetItem("Kingsoul")?.UIDef?.GetSprite();
            VoidHeartIcon = Finder.GetItem("Void_Heart")?.UIDef?.GetSprite();
            if (KingsoulIcon == null || VoidHeartIcon == null)
            {
                Transcendence.Instance.LogWarn("Kingsoul or Void Heart icons not found");
            }
        }
    }
}