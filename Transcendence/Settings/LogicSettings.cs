using MenuChanger.Attributes;

namespace Transcendence
{
    public class LogicSettings
    {
        public bool AntigravityAmulet;
        public GeoCharmLogicMode BluemothWings;
        public bool SnailSoul;
        public bool SnailSlash;
        [MenuLabel("Millibelle's Blessing")]
        public bool MillibellesBlessing;
        public bool NitroCrystal;
        [MenuLabel("Vespa's Vengeance")]
        public bool VespasVengeance;
        public GeoCharmLogicMode Crystalmaster;

        public bool AnyEnabled() =>
            AntigravityAmulet ||
            BluemothWings != GeoCharmLogicMode.Off ||
            SnailSoul ||
            SnailSlash ||
            MillibellesBlessing ||
            NitroCrystal ||
            VespasVengeance ||
            Crystalmaster != GeoCharmLogicMode.Off;
    }

    public enum GeoCharmLogicMode {
        Off,
        OnWithGeo,
        On
    }
}