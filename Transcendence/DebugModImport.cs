using System.Reflection;
using MonoMod.ModInterop;

namespace Transcendence
{
    internal static class DebugMod
    {
        [ModImportName("DebugMod")]
        private static class DebugImport
        {
            public static Action<Action> AddToOnGiveAllCharm;
            public static Action<Action> AddToOnRemoveAllCharm;
        }
        static DebugMod()
        {
            // MonoMod will automatically fill in the actions in DebugImport the first time they're used
            typeof(DebugImport).ModInterop();
        }
        public static void AddToOnGiveAllCharm(Action onGiveCharms) => DebugImport.AddToOnGiveAllCharm?.Invoke(onGiveCharms);
        public static void AddToOnRemoveAllCharm(Action onRemoveCharms) => DebugImport.AddToOnRemoveAllCharm?.Invoke(onRemoveCharms);
    }
}