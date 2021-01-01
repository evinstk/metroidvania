using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System.Collections.Generic;

namespace Game.Editor.Animation
{
    class AnimationData : IResource
    {
        [JsonInclude]
        public string Id { get; set; } = Utils.RandomString();

        public string DisplayName => Name;
        public string Name = "New Animation";
        public int FramesPerSecond = 12;

        public List<Frame> Frames = new List<Frame>();

        public class Frame
        {
            public Sprite Sprite = new Sprite();
            // other animatable things
        }

        public class Sprite
        {
            public string TextureMapId = "";
            public string FrameFilename = "";
        }
    }

    class AnimationManager : Manager<AnimationData>
    {
        public override string Path => ContentPath.Animations;
    }
}
