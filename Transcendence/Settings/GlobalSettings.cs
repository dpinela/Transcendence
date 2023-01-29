namespace Transcendence
{
    public class GlobalSettings
    {
        // We cannot just reuse the RandoSettings type here;
        // if MenuChanger is not installed, the MenuChanger attribute on RandoSettings
        // will prevent (de)serialization from working even though it's not actually
        // used for anything.
        public bool AddCharms = true;
        public int IncreaseMaxCharmCostBy = 7;
        public string LogicSettings = "{}";
        public bool ChaosMode = false;
        public ChaosHudSettings ChaosHud = new();

        public bool EncounteredAntigravityAmulet;
        public bool EncounteredBluemothWings;
        public bool EncounteredLemmsStrength;
        public bool EncounteredSnailSlash;
        public bool EncounteredFloristsBlessing;
        public bool EncounteredShamanAmp;
        public bool EncounteredCrystalmaster;
        public bool EncounteredNitroCrystal;
        public bool EncounteredDisinfectantFlask;
        public bool EncounteredMillibellesBlessing;
        public bool EncounteredGreedsong;
        public bool EncounteredSnailSoul;
        public bool EncounteredChaosOrb;
        public bool EncounteredVespasVengeance;
        public bool EncounteredMarissasAudience;
    }
}