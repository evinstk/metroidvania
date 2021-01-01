using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System.Collections.Generic;

namespace Game.Editor.Animation
{
    class TextureMapData : IResource
    {
        [JsonInclude]
        public string Id => meta.image;
        public string DisplayName => meta.image;

        public List<Frame> frames;
        public Meta meta;
    }

    class Frame
    {
        public string filename;
        [DecodeAlias("frame")]
        public FrameBounds bounds;
        public bool rotated;
        public bool trimmed;
        public FrameBounds spriteSourceSize;
        public SourceSize sourceSize;
        public Vector2 pivot;
    }

    public struct FrameBounds
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

    public class Meta
    {
        public string image;
    }

    public class Vector2
    {
        public float x;
        public float y;
    }

    public class SourceSize
    {
        public int w;
        public int h;
    }
}
