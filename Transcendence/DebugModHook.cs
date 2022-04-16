using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Transcendence
{
    // This needs to be in its own class because for some reason the Hook constructor would do reflection on the
    // Transcendence class, see methods with signatures that reference Randomizer and MenuChanger, and throw an
    // exception if those weren't installed.
    internal static class DebugModHook
    {
        public static void GiveAllCharms(Action a)
        {
            var commands = Type.GetType("DebugMod.BindableFunctions, DebugMod");
            if (commands == null)
            {
                return;
            }
            var method = commands.GetMethod("GiveAllCharms", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return;
            }
            new Hook(
                method,
                (Action orig) => {
                    orig();
                    a();
                }
            );
        }
    }
}