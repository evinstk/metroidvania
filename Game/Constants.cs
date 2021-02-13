using Microsoft.Xna.Framework;
using System.IO;

namespace Game
{
    static class Constants
    {
        public const int ResWidth = 480;
        public const int ResHeight = 270;
        public const int ScreenWidth = 1920;
        public const int ScreenHeight = 1080;

        public readonly static Color ClearColor = new Color(5, 7, 14);
    }

    static class Mask
    {
        public const int Terrain = 1 << 0;
        public const int Player = 1 << 1;
        public const int PlayerAttack = 1 << 2;
    }

    static class ContentPath
    {
#if DEBUG
        public static readonly string Sprites = Path.GetFullPath("../../../Content/Sprites/");
        public static readonly string Maps = "../../../Content/Maps/";
        public static readonly string Tilesets = Path.GetFullPath("../../../Content/Tilesets/");
#endif
    }
}
