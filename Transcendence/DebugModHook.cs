namespace Transcendence
{
    internal static class DebugModHook
    {
        public static void GiveAllCharms(Action a)
        {
            DebugMod.BindableFunctions.OnGiveAllCharms += a;
        }
        
        public static void RemoveAllCharms(Action a)
        {
            DebugMod.BindableFunctions.OnRemoveAllCharms += a;
        }
    }
}