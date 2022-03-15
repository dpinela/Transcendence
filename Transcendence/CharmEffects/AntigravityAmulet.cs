using Modding;
using UnityEngine;
using GlobalEnums;

namespace Transcendence
{
    internal static class AntigravityAmulet
    {
        public static void Hook(Func<bool> equipped)
        {
            Equipped = equipped;
            ModHooks.HeroUpdateHook += ChangeGravity;
        }

        private static Func<bool> Equipped;

        private static void ChangeGravity()
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
            rb.gravityScale = (Equipped() && HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION) ? 0.3f : 0.79f;
        }
    }
}