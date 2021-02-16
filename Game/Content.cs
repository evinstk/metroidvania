using Microsoft.Xna.Framework;
using Nez.Persistence;
using Nez.Systems;
using Nez.Textures;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    static class GameContent
    {
        static Dictionary<string, TextureMap> _textureMaps = new Dictionary<string, TextureMap>();
        static Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        public static Sprite LoadSprite(string pack, string frameName, NezContentManager content)
        {
            if (_sprites.TryGetValue(pack + frameName, out var cachedSprite))
                return cachedSprite;

            var dataPath = ContentPath.Sprites + pack + ".json";
            if (!_textureMaps.TryGetValue(dataPath, out var textureMap))
            {
                var dataStr = File.ReadAllText(dataPath);
                textureMap = Json.FromJson<TextureMap>(dataStr);
                _textureMaps[dataPath] = textureMap;
            }

            var texturePath = ContentPath.Sprites + pack + ".png";
            var texture = content.LoadTexture(texturePath);

            var frame = textureMap.frames.Find(f => f.filename.Contains(frameName + ".png"));
            var sprite = new Sprite(
                texture,
                frame.bounds,
                new Vector2(
                    frame.pivot.x * frame.sourceSize.w,
                    frame.pivot.y * frame.sourceSize.h));

            _sprites[pack + frameName] = sprite;
            return sprite;
        }
    }
}
