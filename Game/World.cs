using System.Collections.Generic;

namespace Game.Tiled
{
    class World
    {
        public List<Map> Maps;
    }

    class Map
    {
        public string FileName;
        public int Height;
        public int Width;
        public int X;
        public int Y;
    }
}
