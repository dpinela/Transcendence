
using System.Collections;
using UnityEngine;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace Transcendence
{
    internal class VespasVengeance : Charm
    {
        public static readonly VespasVengeance Instance = new();

        private VespasVengeance() {}

        public override string Sprite => "VespasVengeance.png";
        public override string Name => "Vespa's Vengeance";
        public override string Description => "A beacon worn by the guardians of the Hive to call for the aid of their kin.\n\nTransforms the Howling Wraiths spell into a swarm of volatile worker bees.";
        public override int DefaultCost => 3;
        public override string Scene => "Hive_01";
        public override float X => 85.0f;
        public override float Y => 10.4f;

        public override CharmSettings Settings(SaveSettings s) => s.VespasVengeance;

        public GameObject Bee;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Knight", "Spell Control", ReplaceScreamWithBEES),
            ("Charm Effects", "Hatchling Spawn", GrabExplosionAssets)
        };

        public override void Hook()
        {
            // ExtraDamageable hardcodes the damage value for each ExtraDamageTypes [sic],
            // so our only way of changing it is to hook the method that applies the damage
            // whenever it is being dealt by our explosions. Since there is *also* no way to
            // check for that in said method, we apply the hook temporarily around
            // DamageEffectTicker.Update, but only if the DamageEffectTicker's owner has
            // our CustomTickDamage component.
            On.DamageEffectTicker.Update += ModifyTickDamage;
        }

        private void ModifyTickDamage(On.DamageEffectTicker.orig_Update orig, DamageEffectTicker self)
        {
            var cd = self.GetComponent<CustomTickDamage>();
            if (cd == null)
            {
                orig(self);
                return;
            }
            void ApplyModifiedExtraDamage(On.ExtraDamageable.orig_ApplyExtraDamageToHealthManager orig, ExtraDamageable self, int damage)
            {
                orig(self, cd.DamagePerTick);
            }
            On.ExtraDamageable.ApplyExtraDamageToHealthManager += ApplyModifiedExtraDamage;
            try
            {
                orig(self);
            }
            finally
            {
                On.ExtraDamageable.ApplyExtraDamageToHealthManager -= ApplyModifiedExtraDamage;
            }
        }

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
                    var n = 10;
                    var damage = ShamanStoneEquipped() ? WraithsShamanDamage : WraithsDamage;
                    var scale = 1.0f;
                    if (HivebloodEquipped())
                    {
                        n *= 2;
                        damage /= 2;
                        scale /= 2;
                    }
                    GameManager.instance.StartCoroutine(Swarm(n, damage, scale, target));
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
                    var n = 10;
                    var damage = ShamanStoneEquipped() ? ShriekShamanDamage : ShriekDamage;
                    var scale = 1.0f;
                    if (HivebloodEquipped())
                    {
                        n *= 2;
                        damage /= 2;
                        scale /= 2;
                    }
                    GameManager.instance.StartCoroutine(Swarm(n, damage, scale, target));
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
        
        private const float BeeAcceleration = 0.3f;

        private static bool ShamanStoneEquipped() =>
            PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_19));

        private static bool HivebloodEquipped() =>
            PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_29));

        private IEnumerator Swarm(int n, int damage, float baseScale, GameObject target)
        {
            var fsmTargetRef = target != null ? new FsmGameObject("") { RawValue = target } : null;
            var accel = new FsmFloat("") { RawValue = BeeAcceleration };
            var dcrestEquipped = PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_10));
            var ampEquipped = ShamanAmp.Instance.Equipped();
            var interval = 2.0f / n;
            for (var i = 0; i < n; i++)
            {
                var here = HeroController.instance.transform.position;
                var b = GameObject.Instantiate(Bee);
                b.SetActive(true);
                b.layer = (int)PhysLayers.HERO_ATTACK;
                if (ampEquipped)
                {
                    ShamanAmp.Enlarge(b);
                }
                if (baseScale != 1)
                {
                    var ls = b.transform.localScale;
                    b.transform.localScale = new Vector3(ls.x * baseScale, ls.y * baseScale, ls.z);
                }
                var bFSM = b.LocateMyFSM("Control");
                bFSM.GetFsmFloat("X Left").Value = here.x - 8;
                bFSM.GetFsmFloat("X Right").Value = here.x + 8;
                bFSM.GetFsmFloat("Start Y").Value = here.y + 10;
                var swarmState = bFSM.GetState("Swarm");
                ((FloatCompare)swarmState.Actions[3]).float2.Value = here.y - 10;
                // The bee will never destroy itself otherwise.
                bFSM.GetState("Reset").AppendAction(() => GameObject.Destroy(b));
                var chase = ((ChaseObjectGround)swarmState.Actions[0]);
                chase.acceleration = accel;
                if (fsmTargetRef != null)
                {
                    // WARNING: Changing the Value property of FsmGameObject this action targets
                    // will wreak havoc on most enemy FSMs - bosses included -, because seemingly all of them
                    // reference the Knight through the same FsmGameObject instance.
                    // Instead, we replace the FsmGameObject object itself. Since each volley of bees targets
                    // the same thing, we can reuse the same FsmGameObject for all of them.
                    chase.target = fsmTargetRef;
                }
                GameObject.Destroy(b.GetComponent<DamageHero>());
                if (dcrestEquipped)
                {
                    var boom = b.AddComponent<OnHitFunc>();
                    boom.OnHit = victim =>
                    {
                        FSMUtility.SendEventToGameObject(victim, "TAKE DAMAGE");
                        HitTaker.Hit(victim, new HitInstance()
                        {
                            Source = b,
                            AttackType = AttackTypes.Spell,
                            CircleDirection = false,
                            DamageDealt = damage,
                            Direction = 0,
                            IgnoreInvulnerable = false,
                            MagnitudeMultiplier = 1.5f,
                            Multiplier = 1f,
                            MoveDirection = false,
                            SpecialType = SpecialTypes.None,
                            IsExtraDamage = false
                        });
                        ExplosionAudioClip.SpawnAndPlayOneShot(ExplosionAudioSourcePrefab, b.transform.position);
                        var exp = GameObject.Instantiate(Explosion);
                        exp.SetActive(true);
                        exp.transform.position = b.transform.position;
                        // The explosion must likewise be forced to destroy itself;
                        // normally it goes back into the object pool.
                        // This is also why we Instantiate it rather than Spawn it;
                        // this prefab is also used by KnightHatchling and we don't want
                        // to accidentally modify the hatchling explosion.
                        exp.LocateMyFSM("Explosion Control").GetState("Recycle")
                            .ReplaceAction(0, () => GameObject.Destroy(exp));
                        var expdmg = exp.AddComponent<CustomTickDamage>();
                        // 5/10/15/20 total damage over 5 ticks
                        // (5/5/5/10 with Hiveblood)
                        expdmg.DamagePerTick = damage < 10 ? 1 : damage / 10;
                        if (ampEquipped)
                        {
                            ShamanAmp.Enlarge(exp);
                        }
                        // Destroy the bee by sending it to the Spell Death state
                        bFSM.SendEvent("SPELL");
                    };
                    boom.enabled = true;
                }
                else
                {
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
                }
                bFSM.SendEvent("SWARM");
                yield return new WaitForSeconds(interval);
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

        private const string ProxyPrefix = "Vespa's Vengeance Proxy-";

        private static GameObject BeaconProxy(GameObject target)
        {
            if (target == null)
            {
                return null;
            }
            var proxy = new GameObject();
            proxy.name = ProxyPrefix + target.name;
            var sync = target.AddComponent<PositionSync>();
            sync.dest = proxy;
            sync.enabled = true;
            proxy.SetActive(true);
            return proxy;
        }

        private AudioSource ExplosionAudioSourcePrefab;
        private AudioEvent ExplosionAudioClip;
        private GameObject Explosion;

        private void GrabExplosionAssets(PlayMakerFSM fsm)
        {
            foreach (var a in fsm.GetState("Hatch").Actions)
            {
                if (a is SpawnObjectFromGlobalPool sa)
                {
                    var kh = sa.gameObject.Value.GetComponent<KnightHatchling>();
                    ExplosionAudioSourcePrefab = kh.audioSourcePrefab;
                    ExplosionAudioClip = kh.dungExplodeSound;
                    Explosion = kh.dungExplosionPrefab;
                    return;
                }
            }
            Transcendence.Instance.LogError("Knight Hatchling spawn action not found. Bee-bombs will not work.");
        }
    }
}