using Modding;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Transcendence
{
    internal static class ShamanAmp
    {
        public static void Hook(Func<bool> equipped, Transcendence mod)
        {
            Equipped = equipped;
            mod.AddFsmEdit("Fireball(Clone)", "Fireball Control", EnlargeVengefulSpirit);
            mod.AddFsmEdit("Fireball2 Spiral(Clone)", "Fireball Control", EnlargeShadeSoul);
            // for Desolate Dive
            mod.AddFsmEdit("Q Slam", "Hit Box Control", EnlargeDive);
            // for Descending Dark
            mod.AddFsmEdit("Q Slam 2", "Hit Box Control", EnlargeDDarkPart1);
            mod.AddFsmEdit("Q Mega", "Hit Box Control", EnlargeDDarkPart2);
            // for Howling Wraiths
            mod.AddFsmEdit("Scr Heads", "Hit Box Control", EnlargeScream);
            // for Abyss Shriek
            mod.AddFsmEdit("Scr Heads 2", "FSM", EnlargeShriek);
        }

        private static Func<bool> Equipped;
        private static bool ShamanStoneEquipped() =>
            PlayerData.instance.GetBool("equippedCharm_19");

        private static float EnlargementFactor() =>
            Math.Max(1.0f, (float)(Math.Pow(PlayerData.instance.GetInt("geo") + 1, 1.0/4)/4.0));

        private static void EnlargeVengefulSpirit(PlayMakerFSM fsm)
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

        private static void EnlargeShadeSoul(PlayMakerFSM fsm)
        {
            fsm.GetState("Set Damage").ReplaceAction(0, () => {
                var scale = 1.8f * (Equipped() ? EnlargementFactor() : 1);
                fsm.gameObject.transform.localScale = new Vector3(scale, scale, 0f);
            });
        }

        private static Vector3? OriginalDiveSize = null;
        private static Vector3? OriginalDDarkPart1Size = null;
        private static Vector3? OriginalDDarkPart2Size = null;

        // We can't capture a ref parameter in a lambda, so we have to repeat ourselves
        // a little bit.
        private static void EnlargeDive(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalDiveSize);
                EnlargeDivePartIfEquipped(obj);
            });
        }

        private static void EnlargeDDarkPart1(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalDDarkPart1Size);
                EnlargeDivePartIfEquipped(obj);
            });
        }

        private static void EnlargeDDarkPart2(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalDDarkPart2Size);
                EnlargeDivePartIfEquipped(obj);
            });
        }

        // Our increase to some object's sizes persists after the scream/dive
        // is done (presumably the game is reusing the object).
        // Keep the original size so we don't end up repeatedy embiggening it.
        private static void RestoreOriginalSize(GameObject obj, ref Vector3? origSize)
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

        private static void EnlargeDivePartIfEquipped(GameObject obj)
        {
            if (Equipped())
            {
                var vec = obj.transform.localScale;
                obj.transform.localScale = new Vector3(vec.x * EnlargementFactor(), vec.y, vec.z);
            }
        }

        private static Vector3? OriginalScreamSize = null;
        private static Vector3? OriginalShriekSize = null;

        private static void EnlargeScream(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Activate").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalScreamSize);
                EnlargeScreamIfEquipped(obj);
            });
        }

        private static void EnlargeShriek(PlayMakerFSM fsm)
        {
            var obj = fsm.gameObject;
            fsm.GetState("Wait").PrependAction(() => {
                RestoreOriginalSize(obj, ref OriginalShriekSize);
                EnlargeScreamIfEquipped(obj);
            });
        }

        private static void EnlargeScreamIfEquipped(GameObject obj)
        {
            if (Equipped())
            {
                var vec = obj.transform.localScale;
                var k = EnlargementFactor();
                obj.transform.localScale = new Vector3(vec.x * k, vec.y * k, vec.z);
            }
        }
    }
}