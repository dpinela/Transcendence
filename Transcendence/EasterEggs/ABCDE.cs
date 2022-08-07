namespace Transcendence
{
    internal class ABCDE
    {
        private static bool Achieved() =>
            PlayerData.instance.GetBool(nameof(PlayerData.brokenCharm_23)) &&
            PlayerData.instance.GetBool(nameof(PlayerData.brokenCharm_24)) &&
            PlayerData.instance.GetBool(nameof(PlayerData.brokenCharm_25)) &&
            PlayerData.instance.GetBool(nameof(PlayerData.hasXunFlower)) &&
            PlayerData.instance.GetBool(nameof(PlayerData.xunFlowerBroken)) &&
            PlayerData.instance.GetBool($"gotCharm_{FloristsBlessing.Instance.Num}") &&
            FloristsBlessing.Instance.Broken;

        public static string? Title() => Achieved() ? "CONGRATULATIONS" : null;

        public static string? Body() => Achieved() ?
            "In destroying all that is fragile and delicate, one proves their unyieldingness.<br>May flowers bloom upon your Shade."  :
            null;
    }
}