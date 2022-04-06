using Modding;

namespace Transcendence
{
    internal class SnailSlash : Charm
    {
        public static readonly SnailSlash Instance = new();

        private SnailSlash() {}

        public override string Sprite => "ShinySlash.png";
        public override string Name => "Snail Slash";
        public override string Description => "A charm symbolising the distaste of shamans for nail combat.\n\nThe bearer will gain SOUL from all sources at a much higher rate, but their nail will deal minimal damage.";
        public override int DefaultCost => 3;
        public override string Scene => "Ruins_Elevator";
        public override float X => 75.1f;
        public override float Y => 96.4f;

        public override CharmSettings Settings(SaveSettings s) => s.SnailSlash;

        public override void Hook()
        {
            ModHooks.GetPlayerIntHook += NerfNail;
            ModHooks.SetPlayerBoolHook += UpdateNailDamageOnEquip;
            On.PlayerData.AddMPCharge += IncreaseSoulCollection;
        }

        private int NerfNail(string intName, int value)
        {
            if (Equipped() && intName == "nailDamage")
            {
                return 1;
            }
            return value;
        }

        private bool IncreaseSoulCollection(On.PlayerData.orig_AddMPCharge orig, PlayerData self, int soul)
        {
            if (Equipped())
            {
                soul *= 2;
            }
            return orig(self, soul);
        }

        private bool UpdateNailDamageOnEquip(string boolName, bool value)
        {
            if (boolName == $"equippedCharm_{Num}")
            {
                Transcendence.UpdateNailDamage();
            }
            return value;
        }
    }
}