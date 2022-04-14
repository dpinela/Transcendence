namespace Transcendence
{
    internal class RandoSettings
    {
        public bool AddCharms = true;
        
        [MenuChanger.Attributes.MenuRange(0, 14)]
        public int IncreaseMaxCharmCostBy = 7;
    }
}