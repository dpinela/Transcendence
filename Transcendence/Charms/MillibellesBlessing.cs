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

        public override List<(int, Action)> Tickers => new() {(TickPeriod, GivePeriodicGeo)};

        private const int TickPeriod = 5;

        private void GivePeriodicGeo()
        {
            if (HeroController.instance != null && Equipped())
            {
                HeroController.instance.AddGeo((int)(3 * Math.Log10(PlayerData.instance.GetInt("geo") + 1)));
            }
        }
    }
}