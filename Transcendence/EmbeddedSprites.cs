using UnityEngine;
using System.Reflection;

namespace Transcendence
{
    internal static class EmbeddedSprites
    {
        private static Dictionary<string, Sprite> Sprites;

        public static void Load()
        {
            var dll = typeof(EmbeddedSprites).Assembly;
            Sprites = dll.GetManifestResourceNames().Where(res => res.EndsWith(".png")).ToDictionary(res => res, res => LoadSprite(dll, res));
        }

        public static Sprite Get(string name)
        {
            return Sprites[name];
        }

        private static Sprite LoadSprite(Assembly dll, String res)
        {
            using var s = dll.GetManifestResourceStream(res);
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, ms.ToArray(), true);
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }
    }
}