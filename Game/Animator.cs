using Microsoft.Xna.Framework;
using Nez.Persistence;
using Nez.Sprites;
using Nez.Systems;
using Nez.Textures;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Game
{
    class TextureMap
    {
        public List<Frame> frames;
    }

    class Frame
    {
        public string filename;
        [DecodeAlias("frame")]
        public FrameBounds bounds;
        public FrameSize sourceSize;
        public FrameVector2 pivot;
    }

    class FrameBounds
    {
        public int x;
        public int y;
        public int w;
        public int h;

        public static implicit operator Rectangle(FrameBounds bounds) => new Rectangle
        {
            X = bounds.x,
            Y = bounds.y,
            Width = bounds.w,
            Height = bounds.h,
        };
    }

    class FrameVector2
    {
        public float x;
        public float y;
    }

    class FrameSize
    {
        public int w;
        public int h;
    }

    static class Animator
    {
        static Regex AlphaNum = new Regex("(?<Name>[a-zA-Z_-]*)(?<Index>[0-9]*)");

        struct AnimFrame
        {
            public int Index;
            public Sprite Sprite;
        }

        public static SpriteAnimator MakeAnimator(string pack, NezContentManager content)
        {
            // TODO: use GameContent
            var dataPath = ContentPath.Sprites + pack + ".json";

            var dataStr = File.ReadAllText(dataPath);
            var data = Json.FromJson<TextureMap>(dataStr);

            var texturePath = ContentPath.Sprites + pack + ".png";
            var texture = content.LoadTexture(texturePath);

            var animations = new Dictionary<string, List<AnimFrame>>();
            foreach (var frame in data.frames)
            {
                var match = AlphaNum.Match(frame.filename);

                var index = match.Groups["Index"];
                if (index.Length == 0) continue;

                var animName = match.Groups["Name"].Value;
                if (!animations.TryGetValue(animName, out var frames))
                {
                    frames = new List<AnimFrame>();
                    animations[animName] = frames;
                }
                frames.Add(new AnimFrame
                {
                    Index = int.Parse(index.Value),
                    Sprite = new Sprite(
                        texture,
                        frame.bounds,
                        new Vector2(frame.pivot.x * frame.sourceSize.w, frame.pivot.y * frame.sourceSize.h)),
                });
            }

            var animator = new SpriteAnimator();
            foreach (var animation in animations)
            {
                var frames = animation.Value;
                frames.Sort((a, b) => a.Index.CompareTo(b.Index));

                // normal
                {
                    var sprites = new Sprite[frames.Count];
                    for (var i = 0; i < sprites.Length; ++i)
                        sprites[i] = frames[i].Sprite;
                    var spriteAnimation = new SpriteAnimation(sprites, 12);
                    animator.AddAnimation(animation.Key, spriteAnimation);
                }

                // reverse
                {
                    var sprites = new Sprite[frames.Count];
                    for (var i = 0; i < sprites.Length; ++i)
                        sprites[i] = frames[sprites.Length - 1 - i].Sprite;
                    var spriteAnimation = new SpriteAnimation(sprites, 12);
                    animator.AddAnimation(animation.Key + "Reverse", spriteAnimation);
                }
            }
            return animator;
        }

        public static void Change(
            this SpriteAnimator animator, string animation, SpriteAnimator.LoopMode loopMode = SpriteAnimator.LoopMode.Loop)
        {
            if (animator.CurrentAnimationName != animation)
                animator.Play(animation, loopMode);
        }
    }
}
