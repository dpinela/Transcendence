using System.Collections;
using Modding;
using UnityEngine;

namespace Transcendence
{
    internal static class BluemothWings
    {
        public static void Hook(Func<bool> equipped, Transcendence mod)
        {
            Equipped = equipped;
            On.HeroController.CanDoubleJump += AllowDoubleJump;
            On.HeroController.DoDoubleJump += AllowExtraJumps;
        }

        private static Func<bool> Equipped;

        private const int ExtraJumpCost = 5;

        private static bool AllowDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
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

        private static void AllowExtraJumps(On.HeroController.orig_DoDoubleJump orig, HeroController self)
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
            self.TakeGeo(ExtraJumpCost);
            GameManager.instance.StartCoroutine(RefreshWings());
        }

        private static IEnumerator RefreshWings()
        {
            yield return new WaitUntil(() => !InputHandler.Instance.inputActions.jump.IsPressed);
            ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
        }
    }
}