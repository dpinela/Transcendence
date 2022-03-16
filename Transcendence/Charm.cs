namespace Transcendence
{
    internal class Charm
    {
        public string Sprite;
        public string Name;
        public string Description;
        public int Cost;
        public Func<SaveSettings, CharmSettings> SettingsBools;
        public Action<Func<bool>> Hook;

        public string Scene;
        public float X;
        public float Y;

        public int Num; // assigned at runtime by SFCore's CharmHelper
    }
}
