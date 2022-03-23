using System;
using Modding;

namespace Transcendence
{
    internal class ChaosOrb : Charm
    {
        public static readonly ChaosOrb Instance = new();

        private ChaosOrb() {}

        public override string Sprite => "ChaosOrb.png";
        public override string Name => "Chaos Orb";
        public override string Description => $"Formed in the depths of the Abyss from fragments of discarded charms.\n\nThe bearer will gain the effects of three random charms, changing over time.\n\nCurrently granting the effects of {GivenCharmDescription()}.";
        public override int DefaultCost => 2;
        public override string Scene => "Deepnest_East_04";
        public override float X => 27.5f;
        public override float Y => 80.4f;

        public override CharmSettings Settings(SaveSettings s) => s.ChaosOrb;

        private const int TickPeriod = 30;

        public override List<(int, Action)> Tickers => new() {(TickPeriod, RerollCharmsIfEquipped)};

        public override void Hook()
        {
            ModHooks.SetPlayerBoolHook += RerollCharmsOnEquip;
            ModHooks.GetPlayerIntHook += EnableKingsoul;
        }

        private List<int> GivenCharms = new();
        public bool GivingCharm(int num) => GivenCharms.Contains(num);

        private string GivenCharmDescription() =>
            GivenCharms.Count switch {
                0 => "nothing",
                1 => Language.Language.Get($"CHARM_NAME_{GivenCharms[0]}", "UI"),
                _ => String.Join(", ", GivenCharms.GetRange(0, GivenCharms.Count - 1).Select(x => Language.Language.Get($"CHARM_NAME_{x}", "UI"))) + " and " + Language.Language.Get($"CHARM_NAME_{GivenCharms[GivenCharms.Count - 1]}", "UI")
            };

        private List<int> CustomCharms = new();
        public void AddCustomCharm(int num) => CustomCharms.Add(num);

        private Random rng = new();

        private List<int> PickNUnequippedCharms(int n)
        {
            var unequippedCharms = new List<int>();
            for (var i = 1; i <= 40; i++)
            {
                // Quick Slash and Elegy are excluded because they don't work.
                if (!(i == 32 || PlayerData.instance.GetBool($"equippedCharm_{i}")))
                {
                    unequippedCharms.Add(i);
                }
            }
            foreach (var i in CustomCharms)
            {
                if (!PlayerData.instance.GetBool($"equippedCharm_{i}"))
                {
                    unequippedCharms.Add(i);
                }
            }
            if (n > unequippedCharms.Count)
            {
                return unequippedCharms;
            }
            for (var i = 0; i < n; i++)
            {
                var pick = rng.Next(unequippedCharms.Count - i);
                var c = unequippedCharms[i + pick];
                unequippedCharms[i + pick] = unequippedCharms[i];
                unequippedCharms[i] = c;
            }
            return unequippedCharms.GetRange(0, n);
        }

        private void RerollCharms()
        {
            GivenCharms = PickNUnequippedCharms(3);
            HeroController.instance.CharmUpdate();
            PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
            PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
        }

        private void RerollCharmsIfEquipped()
        {
            if (Equipped() && HeroController.instance != null)
            {
                RerollCharms();
            }
        }

        private bool RerollCharmsOnEquip(string boolName, bool value)
        {
            // Technically it might be the case that the last tick
            // actually picked zero charms, but in that case every
            // charm is equipped, so rerolling will make no difference.
            if (boolName == $"equippedCharm_{Num}" && value && GivenCharms.Count == 0)
            {
                RerollCharms();
            }
            return value;
        }

        private int EnableKingsoul(string intName, int value)
        {
            if (intName == "royalCharmState" && Equipped() && GivingCharm(36) && value < 3)
            {
                value = 3;
            }
            return value;
        }
    }
}