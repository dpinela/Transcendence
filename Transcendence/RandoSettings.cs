namespace Transcendence
{
    public class RandoSettings
    {
        public bool AddCharms;
        
        [MenuChanger.Attributes.MenuRange(0, 14)]
        public int IncreaseMaxCharmCostBy;

        public RandoSettings(GlobalSettings rs)
        {
            AddCharms = rs.AddCharms;
            IncreaseMaxCharmCostBy = rs.IncreaseMaxCharmCostBy;
        }

        public bool Enabled() => AddCharms;
    }
}