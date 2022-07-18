using UnityEngine;
using System.IO;

namespace Transcendence
{
    internal static class EmbeddedSprites
    {
        private static Dictionary<string, Sprite> Sprites = new();

        public static Sprite Get(string name, float? ppu = null)
        {
            if (Sprites.TryGetValue(name, out var sprite))
            {
                return sprite;
            }
            var tex = LoadTexture(name);
            sprite = (ppu is float x) ?
                Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), x) :
                Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            Sprites[name] = sprite;
            return sprite;
        }

        private static Texture2D LoadTexture(string name)
        {
            var loc = Path.Combine(Path.GetDirectoryName(typeof(EmbeddedSprites).Assembly.Location), name);
            var imageData = File.ReadAllBytes(loc);
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, imageData, true);
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }
    }
}