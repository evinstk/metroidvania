using Microsoft.Xna.Framework;

namespace Data
{
    public class Rect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public class Point
    {
        public int X;
        public int Y;

        public Vector2 ToVector2() => new Vector2(X, Y);
    }
}
