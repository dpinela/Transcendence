namespace Transcendence
{
    public class RandoSettings
    {
        public bool AddCharms;
        
        [MenuChanger.Attributes.MenuRange(0, 14)]
        public int IncreaseMaxCharmCostBy;

        public RandoSettings(RawRandoSettings rs)
        {
            AddCharms = rs.AddCharms;
            IncreaseMaxCharmCostBy = rs.IncreaseMaxCharmCostBy;
        }

        public RawRandoSettings ToRaw()
        {
            return new RawRandoSettings()
            {
                AddCharms = AddCharms,
                IncreaseMaxCharmCostBy = IncreaseMaxCharmCostBy
            };
        }
    }
}