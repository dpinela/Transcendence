using Modding;

namespace Transcendence
{
    internal class Greedsong : Charm
    {
        public static readonly Greedsong Instance = new();

        private Greedsong() {}

        public override string Sprite => "Greedsong.png";
        public override string Name => "Greedsong";
        public override string Description => "A golden charm made by the Grubfather as a gift for his children.\n\nGain Geo when taking damage.";
        public override int DefaultCost => 1;
        public override string Scene => "Ruins2_11";
        public override float X => 51.8f;
        public override float Y => 128.4f;

        public override CharmSettings Settings(SaveSettings s) => s.Greedsong;
        public override void MarkAsEncountered(GlobalSettings s) => s.EncounteredGreedsong = true;

        public override void Hook()
        {
            ModHooks.TakeHealthHook += SpawnGeoOnHit;
        }

        private Random rng = new();

        private static bool GreedEquipped() => PlayerData.instance.GetBool("equippedCharm_24");

        private const int MinGeoOnHit = 1;
        private const int MaxGeoOnHit = 30;
        private const int GreedBonus = 10;

        private int SpawnGeoOnHit(int damage)
        {
            if (Equipped())
            {
                var bonus = GreedEquipped() ? GreedBonus : 0;
                GeoFlinger.Fling(rng.Next(MinGeoOnHit + bonus, MaxGeoOnHit + bonus + 1), HeroController.instance.transform);
            }
            return damage;
        }
    }
}