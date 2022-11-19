
using System.Collections;
using GlobalEnums;
using UnityEngine;
using HutongGames.PlayMaker;
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
                    var target = BeaconProxy(NearestLargeEnemy(HeroController.instance.transform.position));
                    GameManager.instance.StartCoroutine(Swarm(10, ShamanStoneEquipped() ? WraithsShamanDamage : WraithsDamage, target));
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
                    var target = BeaconProxy(NearestLargeEnemy(HeroController.instance.transform.position));
                    GameManager.instance.StartCoroutine(Swarm(10, ShamanStoneEquipped() ? ShriekShamanDamage : ShriekDamage, target));
                }
                else
                {
                    orig2.OnEnter();
                }
            });
        }

        private const int WraithsDamage = 15;
        private const int WraithsShamanDamage = 20;
        private const int ShriekDamage = 30;
        private const int ShriekShamanDamage = 40;

        private static bool ShamanStoneEquipped() =>
            PlayerData.instance.GetBool("equippedCharm_19");

        private IEnumerator Swarm(int n, int damage, GameObject target)
        {
            var fsmTargetRef = target != null ? new FsmGameObject("") { RawValue = target } : null;
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
                var swarmState = bFSM.GetState("Swarm");
                ((FloatCompare)swarmState.Actions[3]).float2.Value = here.y - 10;
                if (fsmTargetRef != null)
                {
                    // WARNING: Changing the Value property of FsmGameObject this action targets
                    // will wreak havoc on most enemy FSMs - bosses included -, because seemingly all of them 
                    // reference the Knight through the same FsmGameObject instance.
                    // Instead, we replace the FsmGameObject object itself. Since each volley of bees targets
                    // the same thing, we can reuse the same FsmGameObject for all of them.
                    ((ChaseObjectGround)swarmState.Actions[0]).target = fsmTargetRef;
                }
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

        private static readonly HashSet<string> PriorityEnemies = new()
        {
            "Mage Lord" // prevents the second phase from being targeted instead of the first
        };

        private static GameObject NearestLargeEnemy(Vector3 playerPos)
        {
            float bestScore = 0;
            GameObject chosenEnemy = null;
            foreach (var hm in UnityEngine.Object.FindObjectsOfType<HealthManager>())
            {
                if (PriorityEnemies.Contains(hm.gameObject.name))
                {
                    return hm.gameObject;
                }
                var score = (float)hm.hp / Vector3.Distance(hm.gameObject.transform.position, playerPos);
                if (score > bestScore)
                {
                    chosenEnemy = hm.gameObject;
                    bestScore = score;
                }
            }
            return chosenEnemy;
        }

        private const string ProxyPrefix = "Hive Beacon Proxy-";

        private static GameObject BeaconProxy(GameObject target)
        {
            if (target == null)
            {
                return null;
            }
            Transcendence.Instance.Log("Hive Beacon targeting " + target.name);
            var proxy = new GameObject();
            proxy.name = ProxyPrefix + target.name;
            var sync = target.AddComponent<PositionSync>();
            sync.dest = proxy;
            sync.enabled = true;
            proxy.SetActive(true);
            return proxy;
        }
    }
}