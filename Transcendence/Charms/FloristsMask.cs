using Modding;
using System.Collections.Generic;

namespace Transcendence
{
    internal class FloristsMask : Charm
    {
        public static readonly FloristsMask Instance = new();

        private FloristsMask() {}

        public override string Sprite => "FloristsMask.png";
        public override string Name => "Florist's Mask";
        public override string Description => "A charm made in the image of the keepers of the Mosskin's lands.\n\nThe bearer may earn Geo by delivering flowers to the denizens of Hallownest.";
        public override int DefaultCost => 1;
        public override string Scene => "Room_Slug_Shrine";
        public override float X => 29.2f;
        public override float Y => 6.4f;

        public override CharmSettings Settings(SaveSettings s) => s.FloristsMask;

        public override void Hook()
        {
            ModHooks.SetPlayerBoolHook += GiveGeoOnFlowerDelivery;
        }

        private const int GeoPerFlower = 8000;

        private static readonly HashSet<string> FlowerDeliveryBools = new() {
            "elderbugGaveFlower",
            "givenGodseekerFlower",
            "givenOroFlower",
            "givenWhiteLadyFlower",
            "givenEmilitiaFlower"
        };

        private bool GiveGeoOnFlowerDelivery(string boolName, bool value)
        {
            if (value && FlowerDeliveryBools.Contains(boolName) && Equipped())
            {
                HeroController.instance.AddGeo(GeoPerFlower);
            }
            return value;
        }
    }
}