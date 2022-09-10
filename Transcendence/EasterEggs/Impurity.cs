using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using TMPro;
using USM = UnityEngine.SceneManagement;
using Modding;
using GlobalEnums;
using ItemChanger;
using System.Collections;

namespace Transcendence
{
    internal class Impurity
    {
        public static void GrantConditionalImmortality(PlayMakerFSM fsm)
        {
            if (fsm.gameObject.scene.name != "Ruins1_04")
            {
                return;
            }

            fsm.GetState("Nail").ReplaceAction(0, () =>
            {
                if (SnailSlash.Instance.Equipped())
                {
                    ShowDreamText("Seriously? That barely tickled me!");
                    // Go back to the initial state so that Sheo's buddy will respond to further
                    // hits.
                    fsm.SetState("Detect");
                }
                else if (LemmsStrength.Instance.Equipped() || FloristsBlessing.Instance.Active())
                {
                    GiveAllSheoItems();
                    WarpToSheo();
                }
                else
                {
                    EuthanizeNailManufacturer("NAIL KILL");
                }
            });

            fsm.GetState("Spell").ReplaceAction(0, () =>
            {
                if (SnailSoul.Instance.Equipped() || ShamanAmp.Instance.Equipped())
                {
                    GiveAllSheoItems();
                    WarpToSheo();
                }
                else
                {
                    EuthanizeNailManufacturer("SPELL KILL");
                }
            });
        }

        private const string SheoHut = "Room_nailmaster_02";

        private static void WarpToSheo()
        {
            // Make sure that Purity runs still end.
            PlayerData.instance.SetBool(nameof(PlayerData.nailsmithKilled), true);
            PlayerData.instance.SetBool(nameof(PlayerData.nailsmithSpared), true);

            // Sheo's hut doesn't have a dream return target, so we have to define one.
            Events.OnSceneChange += CreateDreamWarpTarget;

            // Set the current map zone so that the following death is a dream death
            // and doesn't break Steel Soul files.
            PlayerData.instance.SetVariable(nameof(PlayerData.mapZone), MapZone.DREAM_WORLD);
            GameManager.instance.sm.mapZone = MapZone.DREAM_WORLD;
            PlayerData.instance.SetString(nameof(PlayerData.dreamReturnScene), SheoHut);

            var hc = HeroController.instance;
            var cr = ReflectionHelper.CallMethod<HeroController, IEnumerator>(hc, "Die", new object[] {});
            hc.StartCoroutine(cr);
        }

        private static void GiveAllSheoItems()
        {
            var settings = ItemChanger.Internal.Ref.Settings;
            if (settings != null && settings.Placements.TryGetValue(LocationNames.Great_Slash, out var p))
            {
                p.GiveAll(new()
                {
                    Container = Container.Unknown,
                    FlingType = FlingType.DirectDeposit,
                    MessageType = MessageType.Corner,
                    Callback = item => {}
                });
                p.AddVisitFlag(VisitState.Accepted);
            }
            else
            {
                var pd = PlayerData.instance;
                pd.SetBool(nameof(PlayerData.hasNailArt), true);
                pd.SetBool(nameof(PlayerData.hasDashSlash), true);
                pd.SetBool(nameof(PlayerData.hasAllNailArts),
                    // just in case we were overridden by another mod
                    pd.GetBool(nameof(PlayerData.hasDashSlash)) &&
                    pd.GetBool(nameof(PlayerData.hasCyclone)) &&
                    pd.GetBool(nameof(PlayerData.hasUpwardSlash)));
            }
        }

        private static void CreateDreamWarpTarget(USM.Scene dest)
        {
            try
            {
                if (dest.name == SheoHut)
                {
                    var target = new GameObject();
                    target.name = "Putative Murderer Respawn Point";
                    target.transform.position = new Vector2(26.4f, 4.4f);
                    var transition = target.AddComponent<TransitionPoint>();
                    transition.name = "door_dreamReturn";
                    transition.nonHazardGate = true;

                    target.SetActive(true);
                }
            }
            finally
            {
                Events.OnSceneChange -= CreateDreamWarpTarget;
            }
        }

        private static float LastDreamTextTime = float.NegativeInfinity;

        private const float MinSecondsBetweenDreamText = 5.5f;

        private static void ShowDreamText(string text)
        {
            var now = Time.time;
            if (now < LastDreamTextTime + MinSecondsBetweenDreamText)
            {
                return;
            }
            LastDreamTextTime = now;
            var dreamMsgVar = FsmVariables.GlobalVariables.FindFsmGameObject("Enemy Dream Msg");
            if (dreamMsgVar.Value == null)
            {
                Transcendence.Instance.LogWarn("ShowDreamText: Enemy Dream Msg not available");
                return;
            }
            var fsm = GameObject.Instantiate(dreamMsgVar.Value).LocateMyFSM("Display");
            fsm.GetState("Init").FilterActions(a => a is not SetGameObject);
            fsm.GetState("Check Convo").FilterActions(a => a is not StringCompare);
            fsm.GetState("Set Convo").Actions = new FsmStateAction[0];

            fsm.FsmVariables.FindFsmGameObject("Text").Value.GetComponent<TextMeshPro>().text = text;
            fsm.SendEvent("DISPLAY ENEMY DREAM");
        }

        private static void EuthanizeNailManufacturer(string means)
        {
            var nailsmith = GameObject.Find("Nailsmith Cliff NPC");
            if (nailsmith == null)
            {
                Transcendence.Instance.LogWarn("404 Nailsmith Not Found");
                return;
            }
            nailsmith.LocateMyFSM("Kill").SendEvent(means);
        }
    }
}