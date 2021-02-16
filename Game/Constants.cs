using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
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

        public static NezSpriteFont DefaultFont =>
            new NezSpriteFont(Core.Content.Load<SpriteFont>($"{ContentPath.Fonts}DefaultFont"));
    }

    static class Mask
    {
        public const int Terrain = 1 << 0;
        public const int Player = 1 << 1;
        public const int PlayerAttack = 1 << 2;
        public const int Enemy = 1 << 3;
        public const int Room = 1 << 4;
        public const int Interaction = 1 << 5;
    }

    static class RenderLayer
    {
        public const int Dialog = 100;
        public const int Hud = 101;

        public const int Light = 200;
        public const int LightMap = 201;
    }

    static class Vars
    {
        public const string HudPrompt = "hud_prompt";
        public const string PlayerHealth = "player_health";
        public const string PlayerMaxHealth = "player_max_health";
    }

    static class ContentPath
    {
#if DEBUG
        public static readonly string Sprites = Path.GetFullPath("../../../Content/Sprites/");
        public static readonly string Maps = "../../../Content/Maps/";
        public static readonly string Tilesets = Path.GetFullPath("../../../Content/Tilesets/");
        public static readonly string Fonts = "Fonts/";
        public static readonly string Scripts = "../../../Content/Scripts/";
#endif
    }
}
