namespace Transcendence
{
    // Used as the type of the GlobalSettings for the mod so that it will work without MenuChanger installed
    // (the MenuChanger attribute on RandoSettings will prevent (de)serialization from working even though it's
    // not actually used for anything)
    public class RawRandoSettings
    {
        public bool AddCharms = true;
        public int IncreaseMaxCharmCostBy = 7;
    }
}