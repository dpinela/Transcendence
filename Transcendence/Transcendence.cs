using Modding;
using SFCore;

namespace Transcendence
{
    public class Transcendence : Mod, ILocalSettings<SaveSettings>
    {
        private static List<Charm> Charms = new() {
            new() {
                Sprite = "Transcendence.Resources.LifebloodCore.png",
                Name = "Antigravity Amulet",
                Description = "Used by shamans to float around.\n\nDecreases the effect of gravity on the bearer, allowing them to leap to greater heights.",
                Cost = 3,
                SettingsBools = s => s.AntigravityAmulet,
                Hook = AntigravityAmulet.Hook
            }
        };

        private Dictionary<string, Func<bool>> BoolGetters = new();
        private Dictionary<string, Action<bool>> BoolSetters = new();
        private Dictionary<string, int> Ints = new();

        public override void Initialize()
        {
            EmbeddedSprites.Load();

            foreach (var charm in Charms)
            {
                var num = CharmHelper.AddSprites(EmbeddedSprites.Get(charm.Sprite))[0];
                Ints[$"charmCost_{num}"] = charm.Cost;
                AddTextEdit($"CHARM_NAME_{num}", "UI", charm.Name);
                AddTextEdit($"CHARM_DESC_{num}", "UI", charm.Description);
                var bools = charm.SettingsBools;
                var equipped = () => bools(Settings).Equipped;
                BoolGetters[$"equippedCharm_{num}"] = equipped;
                BoolSetters[$"equippedCharm_{num}"] = value => bools(Settings).Equipped = value;
                BoolGetters[$"gotCharm_{num}"] = () => bools(Settings).Got;
                BoolSetters[$"gotCharm_{num}"] = value => bools(Settings).Got = value;
                BoolGetters[$"newCharm_{num}"] = () => bools(Settings).New;
                BoolSetters[$"newCharm_{num}"] = value => bools(Settings).New = value;
                charm.Hook(equipped);
            }

            ModHooks.GetPlayerBoolHook += ReadCharmBools;
            ModHooks.SetPlayerBoolHook += WriteCharmBools;
            ModHooks.GetPlayerIntHook += ReadCharmCosts;
            ModHooks.LanguageGetHook += GetCharmStrings;
            
        }

        private Dictionary<(string Key, string Sheet), Func<string>> TextEdits = new();

        internal void AddTextEdit(string key, string sheetName, string text)
        {
            TextEdits.Add((key, sheetName), () => text);
        }

        public override string GetVersion() => "1.0";

        private SaveSettings Settings = new();

        public void OnLoadLocal(SaveSettings s)
        {
            Settings = s;
        }

        public SaveSettings OnSaveLocal() => Settings;

        private bool ReadCharmBools(string boolName, bool value)
        {
            if (BoolGetters.TryGetValue(boolName, out var f))
            {
                var v = f();
                Log($"custom {boolName} = ${v}");
                return v;
            }
            Log($"{boolName} = {value}");
            return value;
        }

        private bool WriteCharmBools(string boolName, bool value)
        {
            if (BoolSetters.TryGetValue(boolName, out var f))
            {
                f(value);
            }
            return value;
        }

        private int ReadCharmCosts(string intName, int value)
        {
            if (Ints.TryGetValue(intName, out var cost))
            {
                return cost;
            }
            return value;
        }

        private string GetCharmStrings(string key, string sheetName, string orig)
        {
            if (TextEdits.TryGetValue((key, sheetName), out var text))
            {
                return text();
            }
            return orig;
        }
    }
}
