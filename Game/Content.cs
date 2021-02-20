using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Game
{
    static class GameContent
    {
        static Dictionary<string, TextureMap> _textureMaps = new Dictionary<string, TextureMap>();
        static Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        static Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();

        static Regex AlphaNum = new Regex("(?<Name>[a-zA-Z_-]*)(?<Index>[0-9]*)");

        static GameContent()
        {
            Core.Emitter.AddObserver(CoreEvents.SceneChanged, () =>
            {
                _textureMaps.Clear();
                _sprites.Clear();
                _animations.Clear();
            });
        }


        public static TextureMap LoadTextureMap(string pack)
        {
            var dataPath = ContentPath.Sprites + pack + ".json";
            if (!_textureMaps.TryGetValue(dataPath, out var textureMap))
            {
                var dataStr = File.ReadAllText(dataPath);
                textureMap = Json.FromJson<TextureMap>(dataStr);
                _textureMaps[dataPath] = textureMap;
            }
            return textureMap;
        }

        public static Sprite LoadSprite(string pack, string frameName, NezContentManager content)
        {
            if (_sprites.TryGetValue(pack + frameName, out var cachedSprite))
                return cachedSprite;

            var textureMap = LoadTextureMap(pack);

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

        public static SpriteAnimation LoadAnimation(string pack, string animationName, NezContentManager content)
        {
            var keyName = pack + animationName;
            if (_animations.TryGetValue(keyName, out var cachedAnimation))
                return cachedAnimation;

            var textureMap = LoadTextureMap(pack);

            var texturePath = ContentPath.Sprites + pack + ".png";
            var texture = content.LoadTexture(texturePath);

            var frames = new List<AnimFrame>();
            foreach (var frame in textureMap.frames)
            {
                var match = AlphaNum.Match(frame.filename);
                var animName = match.Groups["Name"].Value;
                if (animName != animationName) continue;
                frames.Add(new AnimFrame
                {
                    Index = int.Parse(match.Groups["Index"].Value),
                    Sprite = new Sprite(
                        texture,
                        frame.bounds,
                        new Vector2(frame.pivot.x * frame.sourceSize.w, frame.pivot.y * frame.sourceSize.h))
                });
            }

            frames.Sort((a, b) => a.Index.CompareTo(b.Index));

            var sprites = new Sprite[frames.Count];
            for (var i = 0; i < sprites.Length; ++i)
                sprites[i] = frames[i].Sprite;
            var spriteAnimation = new SpriteAnimation(sprites, 12);

            return spriteAnimation;
        }

        struct AnimFrame
        {
            public int Index;
            public Sprite Sprite;
        }
    }
}
