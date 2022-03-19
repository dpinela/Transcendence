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
        public override string Description => "Blessed by Ze'mer, one of the Great Knights of Hallownest.\n\nWhile holding a delicate flower, the bearer's nail damage will massively increase.";
        public override int DefaultCost => 1;
        public override string Scene => "Room_Slug_Shrine";
        public override float X => 29.2f;
        public override float Y => 6.4f;

        public override CharmSettings Settings(SaveSettings s) => s.FloristsBlessing;

        public override void Hook()
        {
            ModHooks.GetPlayerIntHook += BuffNail;
            ModHooks.SetPlayerBoolHook += UpdateNailDamage;
        }

        private static bool HasFlower() => PlayerData.instance.GetBool("hasXunFlower") && !PlayerData.instance.GetBool("xunFlowerBroken");

        private const int BuffFactor = 3;

        private int BuffNail(string intName, int value)
        {
            if (intName == "nailDamage" && Equipped() && HasFlower())
            {
                value *= BuffFactor;
            }
            return value;
        }

        private bool UpdateNailDamage(string boolName, bool value)
        {
            if (boolName == "hasXunFlower" || boolName == "xunFlowerBroken" || boolName == $"equippedCharm_{Num}")
            {
                Transcendence.UpdateNailDamage();
            }
            return value;
        }
    }
}