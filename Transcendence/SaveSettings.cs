using FB = Transcendence.FloristsBlessing;

namespace Transcendence
{
    public class SaveSettings
    {
        public CharmSettings AntigravityAmulet = new();
        public CharmSettings BluemothWings = new();
        public CharmSettings LemmsStrength = new();
        public CharmSettings ShinySlash = new();
        public CharmSettings FloristsBlessing = new();
        public bool FloristsBlessingBroken
        {
            get => FB.Instance.Broken;
            set => FB.Instance.Broken = value;
        }
        public CharmSettings ShamanAmp = new();
        public CharmSettings Crystalmaster = new();
        public CharmSettings NitroCrystal = new();
        public CharmSettings DisinfectantFlask = new();
        public CharmSettings MillibellesBlessing = new();
        public CharmSettings Greedsong = new();
        public CharmSettings SnailSoul = new();
        public CharmSettings ChaosOrb = new();
        public CharmSettings MarissasAudience = new();
    }
}