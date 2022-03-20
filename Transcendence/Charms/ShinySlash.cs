using Modding;

namespace Transcendence
{
    internal class ShinySlash : Charm
    {
        public static readonly ShinySlash Instance = new();

        private ShinySlash() {}

        public override string Sprite => "ShinySlash.png";
        public override string Name => "Shiny Slash";
        public override string Description => "Imbues weapons with the power of the bearer's fortune.\n\nNail strikes cost Geo and deal damage proportional to the amount of Geo held.";
        public override int DefaultCost => 3;
        public override string Scene => "Ruins_Elevator";
        public override float X => 75.1f;
        public override float Y => 96.4f;

        public override CharmSettings Settings(SaveSettings s) => s.ShinySlash;

        public override void Hook()
        {
            ModHooks.GetPlayerIntHook += BuffNail;
            ModHooks.SetPlayerBoolHook += UpdateNailDamageOnEquip;
            On.HeroController.AddGeo += UpdateNailDamageOnAddGeo;
            On.HeroController.TakeGeo += UpdateNailDamageOnTakeGeo;
            On.HeroController.DoAttack += PayGeoForAttack;
        }

        private int BuffNail(string intName, int value)
        {
            if (Equipped() && intName == "nailDamage")
            {
                value += PlayerData.instance.GetInt("geo") / 10;
            }
            return value;
        }

        private bool UpdateNailDamageOnEquip(string boolName, bool value)
        {
            if (boolName == $"equippedCharm_{Num}")
            {
                Transcendence.UpdateNailDamage();
            }
            return value;
        }

        private void UpdateNailDamageOnAddGeo(On.HeroController.orig_AddGeo orig, HeroController self, int geo)
        {
            orig(self, geo);
            Transcendence.UpdateNailDamage();
        }

        private void UpdateNailDamageOnTakeGeo(On.HeroController.orig_TakeGeo orig, HeroController self, int geo)
        {
            orig(self, geo);
            Transcendence.UpdateNailDamage();
        }

        private void PayGeoForAttack(On.HeroController.orig_DoAttack orig, HeroController self)
        {
            orig(self);
            if (Equipped())
            {
                self.TakeGeo(PlayerData.instance.GetInt("geo") / 10);
                Transcendence.UpdateNailDamage();
            }
        }
    }
}