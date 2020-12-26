using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor
{
    class RoomScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

            Time.TimeScale = 0;
            ClearColor = new Color(0xff371f0f);

            CreateWindows();
        }

        void CreateWindows()
        {
            CreateEntity("windows")
                .AddComponent<RoomWindow>();

            //CreateEntity("room-data")
            //    .AddComponent<RoomDataComponent>();
        }
    }
}
