using GlobalEnums;

namespace Transcendence
{
    internal class Crystalmaster : Charm
    {
        public static readonly Crystalmaster Instance = new();

        private Crystalmaster() {}

        public override string Sprite => "Crystalmaster.png";
        public override string Name => "Crystalmaster";
        public override string Description => "Bears the likeness of a crystallised bug known only as 'The Crystalmaster'.\n\nGreatly increases the running speed of the bearer, in exchange for Geo. The increase is stronger the richer they are.";
        public override int DefaultCost => 2;
        public override string Scene => "Mines_25";
        public override float X => 28.1f;
        public override float Y => 95.4f;

        public override CharmSettings Settings(SaveSettings s) => s.Crystalmaster;
        public override void MarkAsEncountered(GlobalSettings s) => s.EncounteredCrystalmaster = true;

        public override void Hook()
        {
            On.HeroController.Move += SpeedUp;
        }

        private const string ShopDescription = "I've heard that crystals help you to \"go zoom zoom\", so if that's something you're interested in you should take this beauty home!";

        private const int PhysicsFramesPerSecond = 50;

        private const int ChargeInterval = 5 * PhysicsFramesPerSecond;

        private const float MaxSpeedupFactor = 3.0f;

        private static float SpeedupFactor(int geo) => 1.0f + (MaxSpeedupFactor - 1) * (float)(1.0 - Math.Exp(-geo / 5000.0));

        private static int ChargeTimer = 0;

        private void SpeedUp(On.HeroController.orig_Move orig, HeroController self, float dir)
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