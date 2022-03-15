namespace Transcendence
{
    internal struct Charm
    {
        public string Sprite;
        public string Name;
        public string Description;
        public int Cost;
        public Func<SaveSettings, CharmSettings> SettingsBools;
        public Action<Func<bool>> Hook;
    }
}
