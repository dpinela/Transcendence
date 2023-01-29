namespace Transcendence
{
    internal class MillibellesBlessing : Charm
    {
        public static readonly MillibellesBlessing Instance = new();

        private MillibellesBlessing() {}

        public override string Sprite => "MillibellesBlessing.png";
        public override string Name => "Millibelle's Blessing";
        public override string Description => "A certificate of an investment plan offered by a renowned banker of Hallownest.\n\nMakes the bearer's Geo accumulate interest over time, allowing them to enrich themselves without lifting a finger.";
        public override int DefaultCost => 2;
        public override string Scene => "Fungus3_35";
        public override float X => 7.8f;
        public override float Y => 5.4f;

        public override CharmSettings Settings(SaveSettings s) => s.MillibellesBlessing;
        public override void MarkAsEncountered(GlobalSettings s) => s.EncounteredMillibellesBlessing = true;

        public override List<(int, Action)> Tickers => new() {(TickPeriod, GivePeriodicGeo)};

        private const int TickPeriod = 5;

        private static bool GreedEquipped() => PlayerData.instance.GetBool("equippedCharm_24");

        private void GivePeriodicGeo()
        {
            if (HeroController.instance != null && Equipped())
            {
                var k = GreedEquipped() ? 4 : 3;
                HeroController.instance.AddGeo((int)(k * Math.Log10(PlayerData.instance.GetInt("geo") + 1)));
            }
        }
    }
}