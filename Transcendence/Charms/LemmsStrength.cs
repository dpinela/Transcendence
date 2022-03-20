using Modding;

namespace Transcendence
{
    internal class LemmsStrength : Charm
    {
        public static readonly LemmsStrength Instance = new();

        private LemmsStrength() {}

        public override string Sprite => "LemmsStrength.png";
        public override string Name => "Lemm's Strength";
        public override string Description => "A charm wielded by those who collect relics of Hallownest's past.\n\nThe bearer's nail will be strengthened by each relic they hold.";
        public override int DefaultCost => 3;
        public override string Scene => "Ruins1_27";
        public override float X => 53.6f;
        public override float Y => 23.4f;

        public override CharmSettings Settings(SaveSettings s) => s.LemmsStrength;

        public override void Hook()
        {
            ModHooks.GetPlayerIntHook += BuffNail;
            ModHooks.SetPlayerBoolHook += UpdateNailDamageOnEquip;
            ModHooks.SetPlayerIntHook += UpdateNailDamageOnRelicChange;
        }

        private const int DamagePerJournal = 1;
        private const int DamagePerSeal = 2;
        private const int DamagePerIdol = 4;
        private const int DamagePerEgg = 6;

        private int BuffNail(string intName, int damage)
        {
            if (intName == "nailDamage" && Equipped())
            {
                damage += DamagePerJournal * PlayerData.instance.GetInt("trinket1") + 
                          DamagePerSeal * PlayerData.instance.GetInt("trinket2") +
                          DamagePerIdol * PlayerData.instance.GetInt("trinket3") +
                          DamagePerEgg * PlayerData.instance.GetInt("trinket4");
            }
            return damage;
        }

        private bool UpdateNailDamageOnEquip(string boolName, bool value)
        {
            if (boolName == $"equippedCharm_{Num}")
            {
                Transcendence.UpdateNailDamage();
            }
            return value;
        }
        
        private int UpdateNailDamageOnRelicChange(string intName, int value)
        {
            if (relicInts.Contains(intName) && Equipped())
            {
                Transcendence.UpdateNailDamage();
            }
            return value;
        }

        private static readonly HashSet<string> relicInts = new() {
            "trinket1", "trinket2", "trinket3", "trinket4"
        };
    }
}