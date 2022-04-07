using Modding;
using System.Collections.Generic;

namespace Transcendence
{
    internal class FloristsBlessing : Charm
    {
        public static readonly FloristsBlessing Instance = new();

        private FloristsBlessing() {}

        public override string Sprite => "FloristsBlessing.png";
        public override string Name => "Florist's Blessing";
        public override string Description => $"Blessed by Ze'mer, one of the Great Knights of Hallownest.\n\nMassively increases the damage the bearer deals to enemies with their nail.\n\n{(Broken ? "This charm has broken, and the power inside has been silenced." : "This charm is delicate, and will break if its bearer takes damage.")}";
        public override int DefaultCost => 1;
        public override string Scene => "Room_Slug_Shrine";
        public override float X => 29.2f;
        public override float Y => 6.4f;

        public override CharmSettings Settings(SaveSettings s) => s.FloristsBlessing;

        public bool Broken;

        public override void Hook()
        {
            ModHooks.GetPlayerIntHook += BuffNail;
            ModHooks.TakeHealthHook += BreakOnHit;
            ModHooks.SetPlayerBoolHook += UpdateNailDamage;
        }

        public const int RepairCost = 1200;

        private const int BuffFactor = 3;

        private int BuffNail(string intName, int value)
        {
            if (!Broken && intName == "nailDamage" && Equipped())
            {
                value *= BuffFactor;
            }
            return value;
        }

        private int BreakOnHit(int damage)
        {
            if (!Broken && Equipped() && !ChaosOrb.Instance.GivingCharm(Num))
            {
                Broken = true;
                GameManager.instance.SaveGame();
                ItemChanger.Internal.MessageController.Enqueue(EmbeddedSprites.Get("FloristsBlessingBroken.png"), "Charm Broken");
                Transcendence.UpdateNailDamage();
            }
            return damage;
        }

        private bool UpdateNailDamage(string boolName, bool value)
        {
            if (boolName == $"equippedCharm_{Num}")
            {
                Transcendence.UpdateNailDamage();
            }
            return value;
        }
    }
}