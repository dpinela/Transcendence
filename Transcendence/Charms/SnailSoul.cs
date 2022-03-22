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
            ("Fireball(Clone)", "Fireball Control", SlowVengefulSpirit),
            ("Fireball2 Spiral(Clone)", "Fireball Control", ExtendShadeSoulDuration),
            ("Fireball2 Top(Clone)", "Fireball Cast", SlowShadeSoul)
        };

        private const float Slowdown = 4f;

        private const float VSWait = 0.4f;
        private const float VSVelocity = 40f;
        private const float SSWait = 0.475f;
        private const float SSVelocity = 45f;

        private void SlowVengefulSpirit(PlayMakerFSM fsm)
        {
            var idle = fsm.GetState("Idle");
            var wait = idle.Actions[idle.Actions.Length - 1] as Wait;
            idle.SpliceAction(8, () =>
            {
                if (Equipped())
                {
                    var rb = fsm.gameObject.GetComponent<Rigidbody2D>();
                    rb.velocity = new Vector2(CopySign(VSVelocity, rb.velocity.x) / Slowdown, rb.velocity.y);
                    wait.time.Value = VSWait * Slowdown;
                }
                else
                {
                    wait.time.Value = VSWait;
                }
            });
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
            // Due to the game reusing Shade Soul objects, we can't reliably set
            // their speed from within their FSM. Instead, override the action
            // in the cast FSM that sets the speed. This also has the advantage of
            // being an idempotent patch, so we don't care if the Fireball2 Top
            // object gets recycled.
            var castLeft = fsm.GetState("Cast Left");
            var fbVar = (castLeft.Actions[4] as SpawnObjectFromGlobalPool).storeObject;
            castLeft.ReplaceAction(6, () => {
                var rb = fbVar.Value.GetComponent<Rigidbody2D>();
                rb.velocity = new Vector2(-SSVelocity / (Equipped() ? Slowdown : 1), 0);
            });
            fsm.GetState("Cast Right").ReplaceAction(6, () => {
                var rb = fbVar.Value.GetComponent<Rigidbody2D>();
                rb.velocity = new Vector2(SSVelocity / (Equipped() ? Slowdown : 1), 0);
            });
        }

        private static float CopySign(float x, float sign)
        {
            return Math.Abs(x) * (sign < 0 ? -1 : 1);
        }
    }
}