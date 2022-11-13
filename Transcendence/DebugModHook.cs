using System.Reflection;
using MonoMod.ModInterop;

namespace Transcendence
{
    // This needs to be in its own class because for some reason the Hook constructor would do reflection on the
    // Transcendence class, see methods with signatures that reference Randomizer and MenuChanger, and throw an
    // exception if those weren't installed.
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