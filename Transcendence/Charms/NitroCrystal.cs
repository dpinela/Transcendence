using HutongGames.PlayMaker.Actions;

namespace Transcendence
{
    internal class NitroCrystal : Charm
    {
        public static readonly NitroCrystal Instance = new();

        private NitroCrystal() {}

        public override string Sprite => "NitroCrystal.png";
        public override string Name => "Nitro Crystal";
        public override string Description => "A crystal vessel filled with a dangerously explosive substance.\n\nGreatly increases the speed and damage of the bearer's Super Dashes.";
        public override int DefaultCost => 4;
        public override string Scene => "Mines_13";
        public override float X => 25.6f;
        public override float Y => 21.5f;

        public override CharmSettings Settings(SaveSettings s) => s.NitroCrystal;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("SD Burst", "damages_enemy", IncreaseCDashDamage),
            ("SuperDash Damage", "damages_enemy", IncreaseCDashDamage),
            ("Knight", "Superdash", IncreaseCDashSpeed)
        };

        private const int DamageWhenEquipped = 40;
        private const int DamageWhenUnequipped = 10;

        private void IncreaseCDashDamage(PlayMakerFSM fsm)
        {
            var sendEvent = fsm.GetState("Send Event");
            // Guard against the IntCompare action not being there. That sometimes happens,
            // even though the code works. This is only to keep it from flooding modlog
            // with spurious exceptions.
            var damage = (sendEvent?.Actions[0] as IntCompare)?.integer1;
            if (damage != null && sendEvent != null)
            {
                sendEvent.PrependAction(() => {
                    damage.Value = Equipped() ? DamageWhenEquipped : DamageWhenUnequipped;
                });
            }
        }

        private const int SpeedWhenEquipped = 60;
        private const int SpeedWhenUnequipped = 30;

        private void IncreaseCDashSpeed(PlayMakerFSM fsm)
        {
            var left = fsm.GetState("Left");
            var speed = (left.Actions[0] as SetFloatValue).floatVariable;
            void SetLeftSpeed()
            {
                speed.Value = -(Equipped() ? SpeedWhenEquipped : SpeedWhenUnequipped);
            }
            void SetRightSpeed()
            {
                speed.Value = Equipped() ? SpeedWhenEquipped : SpeedWhenUnequipped;
            }
            left.ReplaceAction(0, SetLeftSpeed);
            fsm.GetState("Right").ReplaceAction(0, SetRightSpeed);
            fsm.GetState("Enter L").ReplaceAction(0, SetLeftSpeed);
            fsm.GetState("Enter R").ReplaceAction(0, SetRightSpeed);
        }
    }
}