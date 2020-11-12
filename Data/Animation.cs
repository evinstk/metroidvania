using Microsoft.Xna.Framework.Content;

namespace Data
{
    public class Animation
    {
        public string Name;
        public string SpriteAtlas;
        public int CellStart;
        public int CellCount;
        [ContentSerializer(Optional = true)]
        public Point ColliderSize;
    }
}
