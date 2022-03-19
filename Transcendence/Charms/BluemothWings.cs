using System.Collections;
using Modding;
using UnityEngine;

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
        }

        private const int ExtraJumpCost = 5;

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