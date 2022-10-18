using MenuChanger.Attributes;

namespace Transcendence
{
    public class LogicSettings
    {
        public bool AntigravityAmulet;
        public GeoCharmLogicMode BluemothWings;
        [MenuLabel("Lemm's Strength")]
        public bool LemmsStrength;
        public int MinimumRelicsRequired;
        [MenuLabel("Florist's Blessing")]
        public bool FloristsBlessing;
        public bool SnailSoul;
        public bool SnailSlash;
        public bool Greedsong;
        [MenuLabel("Millibelle's Blessing")]
        public bool MillibellesBlessing;
        public bool NitroCrystal;
        public GeoCharmLogicMode Crystalmaster;
        [MenuLabel("Marissa's Audience")]
        public bool MarissasAudience;
        public ChaosOrbMode ChaosOrb;
    }

    public enum GeoCharmLogicMode {
        Off,
        OnWithGeo,
        On
    }

    public enum ChaosOrbMode {
        Off,
        Rigged,
        Fair
    }
}