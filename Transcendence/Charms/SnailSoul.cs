using System;
using Modding;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Transcendence
{
    internal class SnailSoul : Charm
    {
        public static readonly SnailSoul Instance = new();

        private SnailSoul() {}

        public override string Sprite => "SnailSoul.png";
        public override string Name => "Snail Soul";
        public override string Description => "A soothing charm worn by shamans on lazy days.\n\nSlows down the bearer's fireballs.";
        public override int DefaultCost => 2;
        public override string Scene => "Fungus3_44";
        public override float X => 10.3f;
        public override float Y => 12.4f;

        public override CharmSettings Settings(SaveSettings s) => s.SnailSoul;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Fireball(Clone)", "Fireball Control", ExtendVengefulSpiritDuration),
            ("Fireball Top(Clone)", "Fireball Cast", SlowVengefulSpirit),
            ("Fireball2 Spiral(Clone)", "Fireball Control", ExtendShadeSoulDuration),
            ("Fireball2 Top(Clone)", "Fireball Cast", SlowShadeSoul)
        };

        private const float Slowdown = 4f;

        private const float VSWait = 0.4f;
        private const float VSVelocity = 40f;
        private const float SSWait = 0.475f;
        private const float SSVelocity = 45f;

        private void ExtendVengefulSpiritDuration(PlayMakerFSM fsm)
        {
            var idle = fsm.GetState("Idle");
            var wait = idle.Actions[idle.Actions.Length - 1] as Wait;
            idle.SpliceAction(8, () =>
            {
                wait.time.Value = VSWait * (Equipped() ? Slowdown : 1);
            });
        }

        private void SlowVengefulSpirit(PlayMakerFSM fsm)
        {
            var castLeft = fsm.GetState("Cast Left");
            // These objects may get reused, so avoid re-patching.
            if (castLeft.Actions[6] is SetVelocityAsAngle sva)
            {
                var speedVar = sva.speed;

                void AdjustSpeed()
                {
                    speedVar.Value = VSVelocity / (Equipped() ? Slowdown : 1);
                }

                castLeft.SpliceAction(6, AdjustSpeed);
                fsm.GetState("Cast Right").SpliceAction(9, AdjustSpeed);
            }
        }

        private void ExtendShadeSoulDuration(PlayMakerFSM fsm)
        {
            var idle = fsm.GetState("Idle");
            // The game reuses Shade Soul objects, but it re-activates their FSMs
            // every time so we must first check whether we've already patched this
            // particular object.
            if (idle.Actions[0] is Wait wait)
            {
                idle.PrependAction(() =>
                {
                    wait.time.Value = SSWait * (Equipped() ? Slowdown : 1);
                });
            }
        }

        private void SlowShadeSoul(PlayMakerFSM fsm)
        {
            var castLeft = fsm.GetState("Cast Left");
            // These objects may get reused, so avoid re-patching.
            if (castLeft.Actions[6] is SetVelocityAsAngle sva)
            {
                var speedVar = sva.speed;

                void AdjustSpeed()
                {
                    speedVar.Value = SSVelocity / (Equipped() ? Slowdown : 1);
                }

                castLeft.SpliceAction(6, AdjustSpeed);
                fsm.GetState("Cast Right").SpliceAction(6, AdjustSpeed);
            }
        }
    }
}