using UnityEngine;

namespace Transcendence
{
    internal static class GeoFlinger
    {
        public static void Fling(int geo, Transform from)
        {
            var numLarge = geo / LargeGeo;
            var rest = geo % LargeGeo;
            var numMedium = rest / MediumGeo;
            var numSmall = rest % MediumGeo;

            LoadPrefabs();
            SpawnAndFling(SmallPrefab, numSmall, from);
            SpawnAndFling(MediumPrefab, numMedium, from);
            SpawnAndFling(LargePrefab, numLarge, from);
        }

        private static GameObject SmallPrefab;
        private static GameObject MediumPrefab;
        private static GameObject LargePrefab;

        public static GameObject Clone(GameObject o)
        {
            var p = UnityEngine.Object.Instantiate(o);
            // No idea why this is necessary.
            UnityEngine.Object.Destroy(p.Spawn());
            p.SetActive(true);
            return p;
        }

        private static void LoadPrefabs()
        {
            if (SmallPrefab != null)
            {
                return;
            }
            var geos = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.StartsWith("Geo"));
            SmallPrefab = geos.First(x => x.name.Contains("Small"));
            MediumPrefab = geos.First(x => x.name.Contains("Med"));
            LargePrefab = geos.First(x => x.name.Contains("Large"));
        }

        private const int LargeGeo = 25;
        private const int MediumGeo = 5;

        private static void SpawnAndFling(GameObject prefab, int n, Transform from)
        {
            if (n != 0)
            {
                prefab = Clone(prefab);
                FlingUtils.SpawnAndFling(new FlingUtils.Config {
                    Prefab = prefab,
                    AmountMin = n,
                    AmountMax = n,
                    SpeedMin = 10f,
                    SpeedMax = 35f,
                    AngleMin = 70f,
                    AngleMax = 125f
                }, from, new Vector3(0f, 0f, 0f));
                prefab.SetActive(false);
            }
        }
    }
}