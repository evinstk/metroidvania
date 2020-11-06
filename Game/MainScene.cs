using Game;
using Microsoft.Xna.Framework;
using Nez;
using System.Linq;

namespace FE
{
	class MainScene : Scene
	{
        static int _triggerLayer = 1;

        public override void Initialize()
        {
            base.Initialize();

            SetDesignResolution(1280, 768, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1280, 768);

            var map = Content.LoadTiledMap("Content/Maps/TestMap.tmx");

            var mapEntity = CreateEntity("map");
            mapEntity.AddComponent(new TiledMapRenderer(map, "terrain"));

            var instanceLayer = map.GetObjectGroup("instances");

            var playerObj = instanceLayer.Objects.First(o => o.Name == "playerSpawn");
            var playerEntity = CreateEntity("player");
            playerEntity.Position = new Vector2(playerObj.X, playerObj.Y);
            playerEntity.AddComponent(new RectangleRenderer(Color.DeepPink, 32, 64));
            playerEntity.AddComponent<ControllerComponent>();
            var playerCollider = playerEntity.AddComponent<BoxCollider>();
            Flags.SetFlagExclusive(ref playerCollider.CollidesWithLayers, 0);

            var triggerLayer = map.GetObjectGroup("triggers");
            var triggersByType = triggerLayer.Objects.ToLookup(t => t.Type);

            foreach (var exit in triggersByType["exit"])
            {
                var exitEntity = CreateEntity(exit.Name != "" ? exit.Name : "exit");
                exitEntity.SetPosition(exit.X, exit.Y);
                var exitCollider = exitEntity.AddComponent(new BoxCollider(exit.X, exit.Y, exit.Width, exit.Height));
                Flags.SetFlagExclusive(ref exitCollider.PhysicsLayer, _triggerLayer);
                exitEntity.AddComponent<ExitTrigger>();
            }

            var cameraEntity = Camera.Entity;
            cameraEntity.AddComponent(new FollowCamera(playerEntity));
            cameraEntity.AddComponent(new CameraComponent(
                Vector2.Zero,
                new Vector2(map.TileWidth * map.Width, map.TileHeight * map.Height)));
        }
    }
}
