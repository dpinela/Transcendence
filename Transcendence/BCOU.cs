namespace Transcendence
{
    internal static class BCOU
    {
        // For unknown reasons, if this hook resides in the Transcendence class, attempting to install it
        // results in a missing assembly error about rando, even though this method does not reference rando
        // in any way, and the Transcendence class contains other hooks on PlayerData that don't cause the
        // same problem.
        public static void BlockChaosOrbUnequip(On.PlayerData.orig_UnequipCharm orig, PlayerData pd, int charmNum)
        {
            if (!(charmNum == ChaosOrb.Instance.Num && Transcendence.Instance.Settings.ChaosMode))
            {
                orig(pd, charmNum);
            }
        }
    }
}