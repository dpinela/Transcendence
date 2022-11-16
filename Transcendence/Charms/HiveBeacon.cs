
using System.Collections;
using GlobalEnums;
using UnityEngine;
using HutongGames.PlayMaker.Actions;

namespace Transcendence
{
    internal class HiveBeacon : Charm
    {
        public static readonly HiveBeacon Instance = new();

        private HiveBeacon() {}

        public override string Sprite => "HiveBeacon.png";
        public override string Name => "Hive Beacon";
        public override string Description => "A device used by the guardians of the Hive to contact each other.\n\nTransforms the Howling Wraiths spell into a swarm of volatile worker bees.";
        public override int DefaultCost => 3;
        public override string Scene => "Hive_01";
        public override float X => 85.0f;
        public override float Y => 10.4f;

        public override CharmSettings Settings(SaveSettings s) => s.HiveBeacon;

        public GameObject Bee;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Knight", "Spell Control", ReplaceScreamWithBEES)
        };

        private void ReplaceScreamWithBEES(PlayMakerFSM fsm)
        {
            var burst1 = fsm.GetState("Scream Burst 1");
            var orig1 = burst1.Actions[7];
            burst1.ReplaceAction(7, () =>
            {
                if (Equipped())
                {
                    if (Bee == null)
                    {
                        Transcendence.Instance.LogError("Bee prefab not loaded");
                        return;
                    }
                    GameManager.instance.StartCoroutine(Swarm(10, ShamanStoneEquipped() ? 20 : 15));
                }
                else
                {
                    orig1.OnEnter();
                }
            });
            var burst2 = fsm.GetState("Scream Burst 2");
            var orig2 = burst2.Actions[8];
            burst2.ReplaceAction(8, () =>
            {
                if (Equipped())
                {
                    if (Bee == null)
                    {
                        Transcendence.Instance.LogError("Bee prefab not loaded");
                        return;
                    }
                    GameManager.instance.StartCoroutine(Swarm(10, ShamanStoneEquipped() ? 40 : 30));
                }
                else
                {
                    orig2.OnEnter();
                }
            });
        }

        private static bool ShamanStoneEquipped() =>
            PlayerData.instance.GetBool("equippedCharm_19");

        private IEnumerator Swarm(int n, int damage)
        {
            for (var i = 0; i < n; i++)
            {
                var here = HeroController.instance.transform.position;
                // and mak'em be affected by dcrest (EXPLOSIONS!)
                var b = GameObject.Instantiate(Bee);
                b.SetActive(true);
                b.layer = (int)PhysLayers.HERO_ATTACK;
                if (ShamanAmp.Instance.Equipped())
                {
                    ShamanAmp.Instance.Enlarge(b);
                }
                var bFSM = b.LocateMyFSM("Control");
                bFSM.GetFsmFloat("X Left").Value = here.x - 15;
                bFSM.GetFsmFloat("X Right").Value = here.x + 15;
                bFSM.GetFsmFloat("Start Y").Value = here.y + 10;
                ((FloatCompare)bFSM.GetState("Swarm").Actions[3]).float2.Value = here.y - 10;
                GameObject.Destroy(b.GetComponent<DamageHero>());
                var damager = b.AddComponent<DamageEnemies>();
                damager.attackType = AttackTypes.Spell;
                damager.circleDirection = false;
                damager.damageDealt = damage;
                damager.direction = 0;
                damager.ignoreInvuln = false;
                damager.magnitudeMult = 1.5f;
                damager.moveDirection = false;
                damager.specialType = SpecialTypes.None;
                damager.enabled = true;
                bFSM.SendEvent("SWARM");
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}