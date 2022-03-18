using Modding;
using System.Collections.Generic;

namespace Transcendence
{
    internal static class FloristsMask
    {
        public static void Hook(Func<bool> equipped, Transcendence mod)
        {
            Equipped = equipped;
            ModHooks.SetPlayerBoolHook += GiveGeoOnFlowerDelivery;
        }

        private static Func<bool> Equipped;

        private const int GeoPerFlower = 8000;

        private static readonly HashSet<string> FlowerDeliveryBools = new() {
            "elderbugGaveFlower",
            "givenGodseekerFlower",
            "givenOroFlower",
            "givenWhiteLadyFlower",
            "givenEmilitiaFlower"
        };

        private static bool GiveGeoOnFlowerDelivery(string boolName, bool value)
        {
            if (value && FlowerDeliveryBools.Contains(boolName) && Equipped())
            {
                HeroController.instance.AddGeo(GeoPerFlower);
            }
            return value;
        }
    }
}