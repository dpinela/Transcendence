using HutongGames.PlayMaker.Actions;

namespace Transcendence
{
    internal static class NitroCrystal
    {
        public static void Hook(Func<bool> equipped, Transcendence mod)
        {
            Equipped = equipped;
            mod.AddFsmEdit("SD Burst", "damages_enemy", IncreaseCDashDamage);
            mod.AddFsmEdit("SuperDash Damage", "damages_enemy", IncreaseCDashDamage);
            mod.AddFsmEdit("Knight", "Superdash", IncreaseCDashSpeed);
        }

        private static Func<bool> Equipped;

        private const int DamageWhenEquipped = 40;
        private const int DamageWhenUnequipped = 10;

        private static void IncreaseCDashDamage(PlayMakerFSM fsm)
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

        private static void IncreaseCDashSpeed(PlayMakerFSM fsm)
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