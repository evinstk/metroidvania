using Nez;
using Game.Tiled;
using Game.Editor;
using System.IO;
using Microsoft.Xna.Framework;

namespace Game
{
    class EditorScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

            Time.TimeScale = 0;
            ClearColor = new Color(0xff371f0f);

            CreateEntity("windows")
                .AddComponent<PrefabWindow>();

            CreateEntity("controller")
                .AddComponent<EditorController>();

            var world = Json.ReadJson<World>("Content/Maps/world8.world");

            foreach (var m in world.Maps)
            {
                var map = Content.LoadTiledMap($"Content/Maps/{m.FileName}");
                var mapEntity = CreateEntity(Path.GetFileNameWithoutExtension(m.FileName));
                mapEntity
                    .SetPosition(m.X, m.Y)
                    .AddComponent(new TiledMapRenderer(map));
            }
        }
    }
}
