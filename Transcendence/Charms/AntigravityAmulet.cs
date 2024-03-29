using Modding;
using UnityEngine;
using GlobalEnums;

namespace Transcendence
{
    internal class AntigravityAmulet : Charm
    {
        public static readonly AntigravityAmulet Instance = new();

        private AntigravityAmulet() {}

        public override string Sprite => "AntigravityAmulet.png";
        public override string Name => "Antigravity Amulet";
        public override string Description => "Used by shamans to float around.\n\nDecreases the effect of gravity on the bearer, allowing them to leap to greater heights and fall more softly.";
        public override int DefaultCost => 2;
        public override string Scene => "Mines_28";
        public override float X => 5.1f;
        public override float Y => 27.4f;

        public override CharmSettings Settings(SaveSettings s) => s.AntigravityAmulet;
        public override void MarkAsEncountered(GlobalSettings s) => s.EncounteredAntigravityAmulet = true;

        public override void Hook()
        {
            ModHooks.HeroUpdateHook += ChangeGravity;
            On.HeroController.ShouldHardLand += FallSoftly;
        }

        private static readonly HashSet<string> InventoryClosedStates = new()
        {
            "Init",
            "Init Enemy List",
            "Closed",
            "Can Open Inventory?",
            "No Inv"
        };

        private static bool InInventory()
        {
            var invState = GameManager.instance?.inventoryFSM?.Fsm.ActiveStateName;
            return invState != null && !InventoryClosedStates.Contains(invState);
        }

        private void ChangeGravity()
        {
            if (HeroController.instance == null)
            {
                return;
            }
            var rb = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
            // Gravity gets set to 0 during transitions; we must not mess with that or
            // the game will hardlock bouncing back and forth between two rooms when
            // passing through a horizontal transition.
            if (rb.gravityScale == 0)
            {
                return;
            }
            // Keep normal gravity after going through upwards transitions, so that the player does not fall
            // through spikes in some rooms before they gain control.
            rb.gravityScale = (Equipped() && !InInventory() && HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION) ? 0.3f : 0.79f;
        }

        private bool FallSoftly(On.HeroController.orig_ShouldHardLand orig, HeroController self, Collision2D collision) =>
            orig(self, collision) && !Equipped();
    }
}