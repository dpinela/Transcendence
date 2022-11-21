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
    }
}