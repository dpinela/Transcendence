using GlobalEnums;

namespace Transcendence
{
    internal static class Crystalmaster
    {
        public static void Hook(Func<bool> equipped, Transcendence mod)
        {
            Equipped = equipped;
            On.HeroController.Move += SpeedUp;
        }

        private static Func<bool> Equipped;

        private const string ShopDescription = "I've heard that crystals help you to \"go zoom zoom\", so if that's something you're interested in you should take this beauty home!";

        private const int PhysicsFramesPerSecond = 50;

        private const int ChargeInterval = 5 * PhysicsFramesPerSecond;

        private const float MaxSpeedupFactor = 5.0f;

        private static float SpeedupFactor(int geo) => 1.0f + (MaxSpeedupFactor - 1) * (float)(1.0 - Math.Exp(-geo / 5000.0));

        private static int ChargeTimer = 0;

        private static void SpeedUp(On.HeroController.orig_Move orig, HeroController self, float dir)
        {
            if (Equipped() && HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION && dir != 0)
            {
                var geo = PlayerData.instance.GetInt("geo");
                dir *= SpeedupFactor(geo);
                ChargeTimer++;
                if (ChargeTimer == ChargeInterval)
                {
                    ChargeTimer = 0;
                    self.TakeGeo(geo / 100);
                }
            }
            orig(self, dir);
        }
    }
}