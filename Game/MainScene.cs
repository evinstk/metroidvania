using Game;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using System.IO;
using System.Linq;

namespace FE
{
	class MainScene : Scene
	{
        string _mapSrc;

        public MainScene(string mapSrc)
        {
            _mapSrc = mapSrc;
        }

        public override void OnStart()
        {
            SetDesignResolution(1280, 768, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1280, 768);

            var map = Content.LoadTiledMap("Content/Maps/" + _mapSrc);

            var mapEntity = CreateEntity("map");
            mapEntity.AddComponent(new TiledMapRenderer(map, "terrain"));

            var instanceLayer = map.GetObjectGroup("instances");

            var playerObj = instanceLayer.Objects.First(o => o.Name == "playerSpawn");
            var playerEntity = CreateEntity("player");
            playerEntity.Position = new Vector2(playerObj.X, playerObj.Y);
            playerEntity.AddComponent(new RectangleRenderer(Color.DeepPink, 32, 64));
            playerEntity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("terrain")));
            playerEntity.AddComponent<ControllerComponent>();
            var playerCollider = playerEntity.AddComponent<BoxCollider>();

            var triggerLayer = map.GetObjectGroup("triggers");
            var triggersByType = triggerLayer.Objects.ToLookup(t => t.Type);

            foreach (var exit in triggersByType["exit"])
            {
                var exitEntity = CreateEntity(exit.Name != "" ? exit.Name : "exit");
                var exitCollider = exitEntity.AddComponent(new BoxCollider(exit.X, exit.Y, exit.Width, exit.Height));
                exitCollider.IsTrigger = true;
                var mapSrc = Path.GetFileName(exit.Properties["map"]);
                exitEntity.AddComponent(new ExitTrigger(mapSrc));
            }

            var cameraEntity = Camera.Entity;
            cameraEntity.AddComponent(new FollowCamera(playerEntity));
            cameraEntity.AddComponent(new CameraComponent(
                new Vector2(map.TileWidth, 0),
                new Vector2(map.TileWidth * (map.Width - 1), map.TileHeight * map.Height)));
        }
    }
}
