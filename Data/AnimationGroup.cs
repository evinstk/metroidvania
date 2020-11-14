using Microsoft.Xna.Framework.Content;

namespace Data
{
    public class AnimationGroup
    {
        public string Name;
        public AnimationType[] AnimationTypes;
    }

    public class AnimationType
    {
        public string Type;
        public string Tileset;
        public string Name;
        [ContentSerializer(Optional = true)]
        public bool Flip;
    }
}
