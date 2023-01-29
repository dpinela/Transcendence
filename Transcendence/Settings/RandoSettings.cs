namespace Transcendence
{
    public class RandoSettings
    {
        public bool AddCharms;
        
        [MenuChanger.Attributes.MenuRange(0, 15)]
        public int IncreaseMaxCharmCostBy;

        public LogicSettings Logic = new();

        public RandoSettings() {}

        public RandoSettings(GlobalSettings rs)
        {
            AddCharms = rs.AddCharms;
            IncreaseMaxCharmCostBy = rs.IncreaseMaxCharmCostBy;
            if (Modding.ModHooks.GetMod("Randomizer 4") != null)
            {
                Logic = LoadLogic(rs);
            }
        }

        private LogicSettings LoadLogic(GlobalSettings rs) =>
            RandomizerMod.RandomizerData.JsonUtil.DeserializeString<LogicSettings>(rs.LogicSettings);

        public bool Enabled() => AddCharms;

        internal void WriteTo(GlobalSettings gs)
        {
            gs.AddCharms = AddCharms;
            gs.IncreaseMaxCharmCostBy = IncreaseMaxCharmCostBy;
            if (Modding.ModHooks.GetMod("Randomizer 4") != null)
            {
                gs.LogicSettings = SaveLogic();
            }
        }

        private string SaveLogic() => RandomizerMod.RandomizerData.JsonUtil.Serialize(Logic);
    }
}