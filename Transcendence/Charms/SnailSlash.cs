using Modding;

namespace Transcendence
{
    internal class SnailSlash : Charm
    {
        public static readonly SnailSlash Instance = new();

        private SnailSlash() {}

        public override string Sprite => "SnailSlash.png";
        public override string Name => "Snail Slash";
        public override string Description => "A charm symbolising the distaste of shamans for nail combat.\n\nThe bearer will gain SOUL from all sources at a much higher rate, but their nail will deal minimal damage.";
        public override int DefaultCost => 3;
        public override string Scene => "Deepnest_45_v02";
        public override float X => 12.5f;
        public override float Y => 42.4f;

        public override CharmSettings Settings(SaveSettings s) => s.SnailSlash;

        public override void Hook()
        {
            ModHooks.GetPlayerIntHook += NerfNail;
            ModHooks.SetPlayerBoolHook += UpdateNailDamageOnEquip;
            // Soul gain through nail hits doesn't go through HeroController.AddMPCharge for some reason.
            // It does go through PlayerData.AddMPCharge, but if we hook that
            // we also duplicate soul transferred from the extra vessels, which
            // is not what we want.
            // The same happens if we hook HeroController.TryAddMPChargeSpa,
            // so it seems that there is no way to speed up soul collection from
            // hot springs and Salubra's Blessing without causing that bug.
            ModHooks.SoulGainHook += IncreaseSoulCollectionFromNail;
            On.HeroController.AddMPCharge += IncreaseSoulCollectionFromElsewhere;
        }

        private int NerfNail(string intName, int value)
        {
            if (Equipped() && intName == "nailDamage")
            {
                return 1;
            }
            return value;
        }

        private int IncreaseSoulCollectionFromNail(int soul)
        {
            if (Equipped())
            {
                soul *= 2;
            }
            return soul;
        }

        private void IncreaseSoulCollectionFromElsewhere(On.HeroController.orig_AddMPCharge orig, HeroController self, int soul)
        {
            if (Equipped())
            {
                soul *= 2;
            }
            orig(self, soul);
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