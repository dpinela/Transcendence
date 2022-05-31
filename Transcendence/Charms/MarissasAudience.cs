using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Modding;
using System.Collections;

namespace Transcendence
{
    internal class MarissasAudience : Charm
    {
        public static readonly MarissasAudience Instance = new();

        private MarissasAudience() {}

        public override string Sprite => "MarissasAudience.png";
        public override string Name => "Marissa's Audience";
        public override string Description => "Prized by those who seek companionship above all else.\n\nThe bearer will be able to summon more companions.";
        public override int DefaultCost => 4;
        public override string Scene => "Ruins_Elevator";
        public override float X => 75.1f;
        public override float Y => 96.4f;

        public override CharmSettings Settings(SaveSettings s) => s.MarissasAudience;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Charm Effects", "Weaverling Control", DoubleWeaverlings),
            ("Charm Effects", "Hatchling Spawn", DoubleHatchlings),
            ("Charm Effects", "Spawn Grimmchild", DoubleGrimmchild),
            ("Grimmchild(Clone)", "Control", MoveDuplicateGrimmchild),
            ("Grimm Scene", "Initial Scene", UpgradeDuplicateGrimmchildTo2),
            ("Defeated NPC", "Conversation Control", UpgradeDuplicateGrimmchildTo3),
            ("Charm Effects", "Spawn Orbit Shield", DoubleDreamshield)
        };

        public override void Hook()
        {
            ModHooks.SetPlayerBoolHook += ToggleDuplicateGrimmchild;
            ChaosOrb.Instance.OnReroll += ToggleDuplicateGrimmchildFromChaosOrb;
        }

        private void DoubleWeaverlings(PlayMakerFSM fsm)
        {
            var spawnExtra = fsm.AddState("Spawn Extra");
            var spawn = fsm.GetState("Spawn");
            spawnExtra.Actions = new FsmStateAction[spawn.Actions.Length];
            Array.Copy(spawn.Actions, spawnExtra.Actions, spawn.Actions.Length);
            spawn.AddTransition("EXTRA", "Spawn Extra");
            spawn.AddAction(() => {
                if (Equipped())
                {
                    fsm.SendEvent("EXTRA");
                }
            });
        }

        private void DoubleHatchlings(PlayMakerFSM fsm)
        {
            var checkCount = fsm.GetState("Check Count");
            var countVar = (checkCount.Actions[0] as GetTagCount).storeResult;
            fsm.GetState("Check Count").ReplaceAction(1, () => {
                var max = Equipped() ? 8 : 4;
                fsm.SendEvent(countVar.Value >= max ? "CANCEL" : "FINISHED");
            });
        }

        private GameObject DuplicateGrimmchild;
        // These will not be available until the first time that Grimmchild is spawned.
        // This should not be a problem; under no circumstances would a duplicate
        // Grimmchild need to spawn before the original.
        private GameObject GrimmchildPrefab;
        private Transform GrimmchildPrefabTransform;

        private void DoubleGrimmchild(PlayMakerFSM fsm)
        {
            var spawn = fsm.GetState("Spawn");
            var origSpawn = spawn.Actions[2] as SpawnObjectFromGlobalPool;
            spawn.SpliceAction(3, () => {
                var spawnPoint = origSpawn.spawnPoint.Value.transform;
                if (GrimmchildPrefab == null)
                {
                    GrimmchildPrefabTransform = spawnPoint;
                    GrimmchildPrefab = origSpawn.gameObject.Value;
                }
                if (Equipped())
                {
                    DuplicateGrimmchild = origSpawn.gameObject.Value.Spawn(spawnPoint.position, spawnPoint.rotation);
                }
            });
        }

        private void SpawnDuplicateGrimmchild()
        {
            if (GrimmchildPrefab == null)
            {
                Transcendence.Instance.LogWarn("cannot spawn duplicate Grimmchild; missing prefab");
                return;
            }
            DuplicateGrimmchild = GrimmchildPrefab.Spawn(GrimmchildPrefabTransform.position, GrimmchildPrefabTransform.rotation);
            FSMUtility.LocateMyFSM(DuplicateGrimmchild, "Control").GetFsmBool("Scene Appear").Value = true;
        }

        private void DespawnDuplicateGrimmchild()
        {
            if (DuplicateGrimmchild != null)
            {
                SendEventToDuplicateGrimmchild("DESPAWN");
                DuplicateGrimmchild = null;
            }
        }

        private static void SendEvent(GameObject obj, string fsmName, string eventName)
        {
            FSMUtility.LocateMyFSM(obj, fsmName).Fsm.Event(new FsmEventTarget() {
                target = FsmEventTarget.EventTarget.GameObject,
                gameObject = new FsmOwnerDefault() { GameObject = new FsmGameObject(obj) }
            }, eventName);
        }

        private void SendEventToDuplicateGrimmchild(string eventName)
        {
            SendEvent(DuplicateGrimmchild, "Charm Unequip", eventName);
        }

        private bool GrimmchildEquipped() => PlayerData.instance.GetBool("equippedCharm_40");
        private bool DreamshieldEquipped() => PlayerData.instance.GetBool("equippedCharm_38");

        private bool ToggleDuplicateGrimmchild(string boolName, bool value)
        {
            if (boolName == $"equippedCharm_{Num}")
            {
                if (GrimmchildEquipped())
                {
                    if (value && DuplicateGrimmchild == null)
                    {
                        SpawnDuplicateGrimmchild();
                    }
                    else if (!value && DuplicateGrimmchild != null)
                    {
                        DespawnDuplicateGrimmchild();
                    }
                }
                
                if (DreamshieldEquipped())
                {
                    if (value && DuplicateDreamshield == null)
                    {
                        SpawnDuplicateDreamshield();
                    }
                    else if (!value && DuplicateDreamshield != null)
                    {
                        DespawnDuplicateDreamshield();
                    }
                }
            }
            return value;
        }

        private static bool JustGrantedCharm(List<int> prevCharms, List<int> newCharms, int num) =>
            !prevCharms.Contains(num) && newCharms.Contains(num);

        private void ToggleDuplicateGrimmchildFromChaosOrb(List<int> prevCharms, List<int> newCharms)
        {
            // Spawn the duplicate Grimmchild if the Orb just granted this charm and Grimmchild is equipped,
            // unless it also just granted Grimmchild, in which case the spawn FSM takes care of this.
            if (JustGrantedCharm(prevCharms, newCharms, Num) && !JustGrantedCharm(prevCharms, newCharms, 40) && GrimmchildEquipped())
            {
                SpawnDuplicateGrimmchild();
            }
            // Despawn the duplicate Grimmchild if the Orb just removed this charm and Grimmchild is equipped, unless
            // it also just granted Grimmchild, in which case there is no duplicate Grimmchild to despawn.
            else if (prevCharms.Contains(Num) && !newCharms.Contains(Num) && !JustGrantedCharm(prevCharms, newCharms, 40) && GrimmchildEquipped())
            {
                DespawnDuplicateGrimmchild();
            }
        }

        private void MoveDuplicateGrimmchild(PlayMakerFSM fsm)
        {
            var change = fsm.GetState("Change");
            // Grimmchild objects can be reused, so check if this one has been patched
            // already.
            if (change.Actions[1] is SetFloatValue s)
            {
                var offsetX = s.floatValue;
                change.PrependAction(() => {
                    offsetX.Value = fsm.gameObject == DuplicateGrimmchild ? 4.5f : 2f;
                });
            }
        }

        private void UpgradeDuplicateGrimmchildTo2(PlayMakerFSM fsm)
        {
            fsm.GetState("Level Up To 2").AppendAction(() => {
                if (Equipped() && DuplicateGrimmchild != null)
                {
                    SendEventToDuplicateGrimmchild("LEVEL UP");
                }
            });
        }

        private void UpgradeDuplicateGrimmchildTo3(PlayMakerFSM fsm)
        {
            fsm.GetState("Level Up To 3").AppendAction(() => {
                if (Equipped() && DuplicateGrimmchild != null)
                {
                    SendEventToDuplicateGrimmchild("LEVEL UP 2");
                }
            });
        }

        private GameObject DreamshieldPrefab;
        private GameObject DuplicateDreamshield;

        private void DoubleDreamshield(PlayMakerFSM fsm)
        {
            var spawn = fsm.GetState("Spawn");
            var origSpawn = spawn.Actions[2] as SpawnObjectFromGlobalPool;
            spawn.SpliceAction(3, () => {
                if (DreamshieldPrefab == null)
                {
                    DreamshieldPrefab = origSpawn.gameObject.Value;
                }
                if (Equipped())
                {
                    var dupeShield = DreamshieldPrefab.Spawn(Vector3.zero, Quaternion.Euler(Vector3.up));
                    dupeShield.transform.Rotate(0, 0, 180);
                    DuplicateDreamshield = dupeShield;
                }
            });
        }

        private void SpawnDuplicateDreamshield()
        {
            if (DreamshieldPrefab == null)
            {
                Transcendence.Instance.LogWarn("cannot spawn duplicate Dreamshield; missing prefab");
                return;
            }
            var dupeShield = DreamshieldPrefab.Spawn(Vector3.zero, Quaternion.Euler(Vector3.up));
            // Put the duplicate shield on the opposite side from the original.
            var origShield = GameObject.FindWithTag("Orbit Shield");
            if (origShield != null)
            {
                dupeShield.transform.rotation = origShield.transform.rotation;
            }
            dupeShield.transform.Rotate(0, 0, 180);
            DuplicateDreamshield = dupeShield;
        }

        private void DespawnDuplicateDreamshield()
        {
            if (DuplicateDreamshield != null)
            {
                var shield = DuplicateDreamshield;
                SendEvent(shield.transform.Find("Shield").gameObject, "Shield Hit", "DISAPPEAR");
                IEnumerator DelayedDestroy()
                {
                    // not sure how long this should be exactly. Orbit Shield's Control FSM says 1 second, but
                    // that's clearly too long; the despawn animation doesn't actually end with the shield gone.
                    yield return new WaitForSeconds(0.5f);
                    shield.Recycle();
                }
                GameManager.instance.StartCoroutine(DelayedDestroy());
                DuplicateDreamshield = null;
            }
        }
    }
}
