using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Modding;

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
                GrimmchildPrefabTransform = spawnPoint;
                GrimmchildPrefab = origSpawn.gameObject.Value;
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
                Transcendence.Instance.LogError("cannot spawn duplicate Grimmchild; missing prefab");
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

        private void SendEventToDuplicateGrimmchild(string eventName)
        {
            FSMUtility.LocateMyFSM(DuplicateGrimmchild, "Charm Unequip").Fsm.Event(new FsmEventTarget() {
                target = FsmEventTarget.EventTarget.GameObject,
                gameObject = new FsmOwnerDefault() { GameObject = new FsmGameObject(DuplicateGrimmchild)}
            }, eventName);
        }

        private bool GrimmchildEquipped() => PlayerData.instance.GetBool("equippedCharm_40");

        private bool ToggleDuplicateGrimmchild(string boolName, bool value)
        {
            if (boolName == $"equippedCharm_{Num}" && GrimmchildEquipped())
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

        private void DoubleDreamshield(PlayMakerFSM fsm)
        {
            var spawn = fsm.GetState("Spawn");
            var origSpawn = spawn.Actions[2] as SpawnObjectFromGlobalPool;
            spawn.SpliceAction(3, () => {
                if (Equipped())
                {
                    var dupeShield = origSpawn.gameObject.Value.Spawn(Vector3.zero, Quaternion.Euler(Vector3.up));
                    dupeShield.transform.Rotate(0, 0, 180);
                }
            });
        }
    }
}
