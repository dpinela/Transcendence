using System.Collections;
using System.Reflection;
using Modding;
using GlobalEnums;
using UnityEngine;
using USM = UnityEngine.SceneManagement;
using ItemChanger;

namespace Transcendence
{
    internal class BluemothWings : Charm
    {
        public static readonly BluemothWings Instance = new();

        private BluemothWings() {}

        public override string Sprite => "BluemothWings.png";
        public override string Name => "Bluemoth Wings";
        public override string Description => "A charm made from the wings of a rare blue bug.\n\nAllows the bearer to jump repeatedly in the air in exchange for Geo.";
        public override int DefaultCost => 2;
        public override string Scene => "Fungus1_17";
        public override float X => 71.5f;
        public override float Y => 24.4f;

        public override CharmSettings Settings(SaveSettings s) => s.BluemothWings;

        public override void Hook()
        {
            On.HeroController.CanDoubleJump += AllowDoubleJump;
            On.HeroController.DoDoubleJump += AllowExtraJumps;

            Events.OnSceneChange += OpenOnewayTransitions;
            On.HeroController.AffectedByGravity += GiveControlFromUpwardsTransitions;
        }

        private const int ExtraJumpCost = 10;

        private bool AllowDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
        {
            // We pretend to have wings only during this call so that a rando wings
            // pickup doesn't think we already have wings and gives us the dupe
            // instead.
            bool PretendToHaveWings(string boolName, bool value) =>
                boolName == "hasDoubleJump" ? (value || Equipped()) : value;

            ModHooks.GetPlayerBoolHook += PretendToHaveWings;
            var result = orig(self);
            ModHooks.GetPlayerBoolHook -= PretendToHaveWings;
            return result;
        }

        private void AllowExtraJumps(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            if (!Equipped())
            {
                orig(self);
                return;
            }
            if (PlayerData.instance.GetInt("geo") < ExtraJumpCost)
            {
                return;
            }
            self.dJumpWingsPrefab.SetActive(false);
            self.dJumpFlashPrefab.SetActive(false);
            orig(self);

            // In Godseeker mode, there is no geo counter, so TakeGeo will throw an exception.
            if (PlayerData.instance.GetBool("bossRushMode"))
            {
                PlayerData.instance.IntAdd("geo", -ExtraJumpCost);
            }
            else
            {
                self.TakeGeo(ExtraJumpCost);
            }
            
            GameManager.instance.StartCoroutine(RefreshWings());
        }

        private static IEnumerator RefreshWings()
        {
            yield return new WaitUntil(() => !InputHandler.Instance.inputActions.jump.IsPressed);
            ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
        }

        private static readonly Dictionary<string, string> DisabledUpwardsTransitions = new()
        {
            {"RestingGrounds_02", "top1"}, // also reachable with Antigravity, but cannot go further
            {"Mines_13", "top1"}, 
            {"Mines_23", "top1"}, // also reachable with Antigravity, but cannot go further
            {"Town", "_Transition Gates/top1"},
            {"Tutorial_01", "_Transition Gates/top1"},
            {"Fungus2_25", "top2"}, // also reachable with Antigravity; can reach a platform with either another jump or a dash; not working yet; collider not active when entering room from the right?
            {"Deepnest_East_03", "top2"}, // also reachable with Antigravity; can reach a platform with another jump
            {"Deepnest_01b", "_Transition Gates/top2"}
        };

        private static readonly Dictionary<string, string> OnewayUpwardsTargets = new()
        {
            {"Mines_28", "bot1"},
            {"Mines_34", "bot2"},
            {"Cliffs_02", "bot2"},
            {"Fungus2_30", "bot1"},
            {"Deepnest_East_07", "bot2"},
            {"Deepnest_01", "bot2"}
        };

        private void OpenOnewayTransitions(USM.Scene dest)
        {
            if (DisabledUpwardsTransitions.TryGetValue(dest.name, out var gateName))
            {
                var coll = GameObject.Find(gateName)?.GetComponent<Collider2D>();
                if (coll == null)
                {
                    Transcendence.Instance.LogWarn($"collider for gate {gateName} in scene {dest.name} not found");
                }
                else
                {
                    coll.enabled = true;
                }
            }
        }

        private void GiveControlFromUpwardsTransitions(On.HeroController.orig_AffectedByGravity orig, HeroController self, bool gravityOn)
        {
            // AffectedByGravity(true) is called by EnterScene under the below conditions
            // when entering a scene from below.
            // We use that as a hook to give back control because it's far less cursed than
            // trying to hook EnterScene itself (which is a coroutine method).
            if (gravityOn &&
                self.transitionState == HeroTransitionState.ENTERING_SCENE &&
                self.sceneEntryGate?.GetGatePosition() == GatePosition.bottom &&
                OnewayUpwardsTargets.TryGetValue(GameManager.instance.sceneName, out var gate) &&
                self.GetEntryGateName() == gate)
            {
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

                Transcendence.Instance.Log("returning control on upwards transition");
                typeof(HeroController).GetMethod("FinishedEnteringScene", flags)?.Invoke(self, new object[] { true, false });
                // After this call to AffectedByGravity, EnterScene sets transitionState
                // to DROPPING_DOWN, which gives the player invulnerability and also disables Antigravity
                // This is the easiest and most reliable way of fixing that.
                Transcendence.DoNextFrame(() => self.transitionState = HeroTransitionState.WAITING_TO_TRANSITION);
            }
            orig(self, gravityOn);
        }
    }
}