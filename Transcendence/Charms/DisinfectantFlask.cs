using Modding;
using UnityEngine;
using System.Collections.Generic;
using USM = UnityEngine.SceneManagement;

namespace Transcendence
{
    internal class DisinfectantFlask : Charm
    {
        public static readonly DisinfectantFlask Instance = new();

        private DisinfectantFlask() {}

        public override string Sprite => "DisinfectantFlask.png";
        public override string Name => "Disinfectant Flask";
        public override string Description => "A vessel containing pure, concentrated lifeblood.\n\nCleans infected areas around the bearer.";
        public override int DefaultCost => 1;
        public override string Scene => "Deepnest_East_15";
        public override float X => 30.8f;
        public override float Y => 4.4f;

        public override CharmSettings Settings(SaveSettings s) => s.DisinfectantFlask;

        public override void Hook()
        {
            ModHooks.GetPlayerBoolHook += DisinfectCrossroads;
            USM.SceneManager.activeSceneChanged += DisinfectOtherAreas;
        }

        private bool DisinfectCrossroads(string boolName, bool value)
        {
            if (boolName == "crossroadsInfected")
            {
                value = value && !Equipped();
            }
            return value;
        }

        private static readonly HashSet<string> DisinfectedScenes = new() {
            "Abyss_17",
            "Abyss_19",
            "Abyss_20",
            "Crossroads_21",
            "Crossroads_22",
            "Waterways_03",
            "Room_Final_Boss_Atrium",
            "Room_Final_Boss_Core"
        };

        private static readonly List<string> DisinfectedPrefixes = new() {
            "infected_vine",
            "infected_large_blob",
            "infected_orange_drip",
            "infected_floor_",
            "infected_crossroads_particles",
            "infected_dark_blob",
            "Infected Flag",
            "Audio Orange Pulse",
            "Pulse Audio",
            "Parasite Balloon",
            "Lesser Mawlek",
            "Mawlek Turret",
            "Scuttler Spawn",
            "Scuttler Group",
            "Battle Gate Deepnest",
            "wispy smoke BG",
        };

        private void DisinfectOtherAreas(USM.Scene from, USM.Scene to)
        {
            if (Equipped() && DisinfectedScenes.Contains(to.name))
            {
                foreach (var obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    foreach (var p in DisinfectedPrefixes)
                    {
                        if (obj.name.StartsWith(p))
                        {
                            UnityEngine.Object.Destroy(obj);
                        }
                    }
                }
            }
        }
    }
}