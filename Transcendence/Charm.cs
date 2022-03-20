namespace Transcendence
{
    internal abstract class Charm
    {
        public abstract string Sprite { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int DefaultCost { get; }
        public abstract string Scene { get; }
        public abstract float X { get; }
        public abstract float Y { get; }

        // assigned at runtime by SFCore's CharmHelper
        public int Num { get; set; }

        public bool Equipped() => PlayerData.instance.GetBool($"equippedCharm_{Num}");

        public abstract CharmSettings Settings(SaveSettings s);

        public virtual void Hook() {}
        public virtual List<(string obj, string fsm, Action<PlayMakerFSM> edit)> FsmEdits => new();
        public virtual List<(int Period, Action Func)> Tickers => new();
    }
}
