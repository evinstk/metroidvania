using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Data.TextureMap
{
    public class TextureMap
    {
        public List<Frame> Frames;
        public Meta Meta;

        static Regex NumAlpha = new Regex("(?<Name>[a-zA-Z]*)(?<Index>[0-9]*)");
        public ILookup<string, Frame> GetGroups()
        {
            var dict = Frames
                .Select(Frame => new { Match = NumAlpha.Match(Frame.Filename), Frame })
                .ToLookup(f => f.Match.Groups["Name"].Value, f => f.Frame);
            return dict;
        }
    }

    public class Frame
    {
        public string Filename;
        [JsonProperty("frame")]
        public FrameBounds Bounds;
        public bool Rotated;
        public bool Trimmed;
        public Vector2 Pivot;
    }

    public class FrameBounds
    {
        public int X;
        public int Y;
        public int W;
        public int H;
    }

    public class Meta
    {
        public string Image;
    }

    public class Vector2
    {
        public float X;
        public float Y;
    }
}
