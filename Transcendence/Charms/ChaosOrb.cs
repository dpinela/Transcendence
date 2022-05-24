using System;
using Modding;
using Vector2 = UnityEngine.Vector2;
using ItemChanger;

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
            ModHooks.GetPlayerBoolHook += EnableUnbreakableCharms;
            // Temporarily stop giving any charms while ItemChanger is giving an item. This solves two issues:
            // - While picking up a WhiteFragmentItem, having royalCharmState raised to 3 from EnableKingsoul would cause
            // the fragment to erroneously turn into Void Heart.
            // - While picking up a Fragile/Unbreakable charm, having it set to unbreakable from EnableUnbreakableCharms
            // would cause... problems. (Probably, the Fragile version would be given as normal, but the Unbreakable
            // pickup would be treated as a duplicate as the game thinks you already have it.)
            // - When NotchCostUI is active, picking up a charm that is currently being granted by the Orb would make
            // the pickup message display the cost as 0 instead of the charm's actual cost.
            AbstractItem.ModifyItemGlobal += DisableWhileGivingItem;
        }

        public List<int> GivenCharms = new();
        public bool GivingCharm(int num) => GivenCharms.Contains(num);

        private string GivenCharmDescription() =>
            GivenCharms.Count switch {
                0 => "nothing",
                1 => CharmName(GivenCharms[0]),
                _ => String.Join(", ", GivenCharms.GetRange(0, GivenCharms.Count - 1).Select(CharmName)) + " and " + CharmName(GivenCharms[GivenCharms.Count - 1])
            };

        private static string CharmName(int num) {
            var key = num switch {
                36 => PlayerData.instance.GetInt("royalCharmState") > 3 ? "CHARM_NAME_36_C" : "CHARM_NAME_36_B",
                23 or 24 or 25 => $"CHARM_NAME_{num}_G",
                _ => $"CHARM_NAME_{num}"
            };
            return Language.Language.Get(key, "UI");
        }

        private List<int> CustomCharms = new();
        public void AddCustomCharm(int num) => CustomCharms.Add(num);

        private Random rng = new();

        private List<int> PickNUnequippedCharms(int n)
        {
            var unequippedCharms = new List<int>();
            for (var i = 1; i <= 40; i++)
            {
                // Joni's Blessing is excluded because it causes wonky behaviour
                // when given by this charm.
                if (!(i == 27 || PlayerData.instance.GetBool($"equippedCharm_{i}")))
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
            GivenCharms.Clear(); // so that charms currently given by the Orb can be selected again
            GivenCharms = PickNUnequippedCharms(3);
            PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
            PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            if (GivenCharms.Contains(6))
            {
                if (Health() == 1)
                {
                    PlayMakerFSM.BroadcastEvent("ENABLE FURY");
                }
                
            }
            // what if the player equipped it manually while Orb was giving it?
            else if (!PlayerData.instance.GetBool("equippedCharm_6"))
            {
                PlayMakerFSM.BroadcastEvent("DISABLE FURY");
            }
        }

        private static int Health() => 
            PlayerData.instance.GetInt(PlayerData.instance.GetBool("equippedCharm_27") ? "joniHealthBlue" : "health");

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

        private bool EnableUnbreakableCharms(string boolName, bool value) =>
            boolName switch {
                "fragileHealth_unbreakable" => value || (Equipped() && GivingCharm(23)),
                "fragileGreed_unbreakable" => value || (Equipped() && GivingCharm(24)),
                "fragileStrength_unbreakable" => value || (Equipped() && GivingCharm(25)),
                _ => value
            };

        private static readonly List<int> empty = new();

        private void DisableWhileGivingItem(GiveEventArgs args)
        {
            // args.Info may be null in contexts where no item is being given.
            if (args == null || args.Info == null)
            {
                return;
            }
            var givenCharms = GivenCharms;
            GivenCharms = empty;
            args.Info.Callback += (AbstractItem it) => {
                GivenCharms = givenCharms;
            };
        }
    }
}