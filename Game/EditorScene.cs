using Nez;
using Game.Tiled;
using System.IO;

namespace Game
{
    class EditorScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

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
