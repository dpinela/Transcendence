using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace Transcendence
{
    internal class MarissasAudience : Charm
    {
        public static readonly MarissasAudience Instance = new();

        private MarissasAudience() {}

        public override string Sprite => "MarissasAudience.png";
        public override string Name => "Marissa's Audience";
        public override string Description => "Prized by those who seek companionship above all else.\n\nThe bearer will be able to summon more companions.";
        public override int DefaultCost => 4;
        public override string Scene => "Ruins_Elevator";
        public override float X => 75.1f;
        public override float Y => 96.4f;

        public override CharmSettings Settings(SaveSettings s) => s.MarissasAudience;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Charm Effects", "Weaverling Control", DoubleWeaverlings),
            ("Charm Effects", "Hatchling Spawn", DoubleHatchlings)
        };

        private void DoubleWeaverlings(PlayMakerFSM fsm)
        {
            var spawnExtra = fsm.AddState("Spawn Extra");
            var spawn = fsm.GetState("Spawn");
            spawnExtra.Actions = new FsmStateAction[spawn.Actions.Length];
            Array.Copy(spawn.Actions, spawnExtra.Actions, spawn.Actions.Length);
            spawn.AddTransition("EXTRA", "Spawn Extra");
            spawn.AddAction(() => {
                if (Equipped())
                {
                    fsm.SendEvent("EXTRA");
                }
            });
        }

        private void DoubleHatchlings(PlayMakerFSM fsm)
        {
            var checkCount = fsm.GetState("Check Count");
            var countVar = (checkCount.Actions[0] as GetTagCount).storeResult;
            fsm.GetState("Check Count").ReplaceAction(1, () => {
                var max = Equipped() ? 8 : 4;
                fsm.SendEvent(countVar.Value >= max ? "CANCEL" : "FINISHED");
            });
        }
    }
}
