using Modding;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Transcendence
{
    internal class ShamanAmp : Charm
    {
        public static readonly ShamanAmp Instance = new();

        private ShamanAmp() {}

        public override string Sprite => "ShamanAmp.png";
        public override string Name => "Shaman Amp";
        public override string Description => "Forgotten shaman artifact, used by wealthy shamans to strike fear in foes.\n\nIncreases the size of spells in proportion to the amount of Geo held.";
        public override int DefaultCost => 4;
        public override string Scene => "Room_GG_Shortcut";
        public override float X => 103.3f;
        public override float Y => 69.4f;

        public override CharmSettings Settings(SaveSettings s) => s.ShamanAmp;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Fireball(Clone)", "Fireball Control", EnlargeVengefulSpirit),
            ("Fireball2 Spiral(Clone)", "Fireball Control", EnlargeShadeSoul),
            ("Q Slam", "Hit Box Control", EnlargeDive),
            ("Q Slam 2", "Hit Box Control", EnlargeDDarkPart1),
            ("Q Mega", "Hit Box Control", EnlargeDDarkPart2),
            ("Scr Heads", "Hit Box Control", EnlargeScream),
            ("Scr Heads 2", "FSM", EnlargeShriek)
        };

        public override void Hook()
        {
            ModHooks.ObjectPoolSpawnHook += EnlargeFlukes;
        }

        private static bool ShamanStoneEquipped() =>
            PlayerData.instance.GetBool("equippedCharm_19");

        private static float EnlargementFactor() =>
            Math.Max(1.0f, (float)(Math.Pow(PlayerData.instance.GetInt("geo") + 1, 1.0/4)/4.0));

        private void EnlargeVengefulSpirit(PlayMakerFSM fsm)
        {
            var setDamage = fsm.GetState("Set Damage");
            setDamage.ReplaceAction(0, () => {
                var scaleX = 1.0f;
                var scaleY = 1.0f;
                if (Equipped())
                {
                    var k = EnlargementFactor();
                    scaleX *= k;
                    scaleY *= k;
                }
                if (ShamanStoneEquipped())
                {
                    scaleX *= 1.3f;
                    scaleY *= 1.6f;
                }
                fsm.gameObject.transform.localScale = new Vector3(scaleX, scaleY, 0f);
            });
            // this is normally the action that increases the size when Shaman Stone
            // is equipped; we already covered that above.
            setDamage.ReplaceAction(6, () => {});
        }

        private void EnlargeShadeSoul(PlayMakerFSM fsm)
        {
            fsm.GetState("Set Damage").ReplaceAction(0, () => {
                var scale = 1.8f * (Equipped() ? EnlargementFactor() : 1);
                fsm.gameObject.transform.localScale = new Vector3(scale, scale, 0f);
            });
        }

        private Vector3? OriginalDiveSize = null;
        private Vector3? OriginalDDarkPart1Size = null;
        private Vector3? OriginalDDarkPart2Size = null;

        // We can't capture a ref parameter in a lambda, so we have to repeat ourselves
        // a little bit.
        private void EnlargeDive(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalDiveSize);
                EnlargeDivePartIfEquipped(obj);
            });
        }

        private void EnlargeDDarkPart1(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalDDarkPart1Size);
                EnlargeDivePartIfEquipped(obj);
            });
        }

        private void EnlargeDDarkPart2(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalDDarkPart2Size);
                EnlargeDivePartIfEquipped(obj);
            });
        }

        // Our increase to some object's sizes persists after the scream/dive
        // is done (presumably the game is reusing the object).
        // Keep the original size so we don't end up repeatedly embiggening it.
        private void RestoreOriginalSize(GameObject obj, ref Vector3? origSize)
        {
            if (origSize is Vector3 v)
            {
                obj.transform.localScale = v;
            }
            else
            {
                origSize = obj.transform.localScale;
            }
        }

        private void EnlargeDivePartIfEquipped(GameObject obj)
        {
            if (Equipped())
            {
                var vec = obj.transform.localScale;
                obj.transform.localScale = new Vector3(vec.x * EnlargementFactor(), vec.y, vec.z);
            }
        }

        private Vector3? OriginalScreamSize = null;
        private Vector3? OriginalShriekSize = null;

        private void EnlargeScream(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalScreamSize);
                EnlargeScreamIfEquipped(obj);
            });
        }

        private void EnlargeShriek(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Wait").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalShriekSize);
                EnlargeScreamIfEquipped(obj);
            });
        }

        private void EnlargeScreamIfEquipped(GameObject obj)
        {
            if (Equipped())
            {
                Enlarge(obj);
            }
        }

        private GameObject EnlargeFlukes(GameObject obj)
        {
            if (obj.name.StartsWith("Spell Fluke") && Equipped())
            {
                Enlarge(obj);
            }
            return obj;
        }

        public void Enlarge(GameObject obj)
        {
            var vec = obj.transform.localScale;
            var k = EnlargementFactor();
            obj.transform.localScale = new Vector3(vec.x * k, vec.y * k, vec.z);
        }
    }
}