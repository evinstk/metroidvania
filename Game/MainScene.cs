using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using System.IO;
using System.Linq;

namespace Game
{
	class MainScene : Scene
	{
        string _mapSrc;
        string _spawn;

        static int resWidth = 1920;
        static int resHeight = 1080;

        public TmxMap Map { get; private set; }

        public MainScene(string mapSrc, string spawn)
        {
            _mapSrc = mapSrc;
            _spawn = spawn;
        }

        public override void OnStart()
        {
            SetDesignResolution(resWidth, resHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(resWidth, resHeight);

            Map = Content.LoadTiledMap("Content/Maps/" + _mapSrc);

            var mapEntity = CreateEntity("map");
            mapEntity.AddComponent(new TiledMapRenderer(Map, "terrain"));

            var instanceLayer = Map.GetObjectGroup("instances");

            var playerObj = instanceLayer.Objects.First(o => o.Name == _spawn && o.Type == "playerSpawn");
            var playerEntity = Mob.MakeMobEntity("player", "Hero", new MobOptions
            {
                PlayerControlled = true,
            });
            playerEntity.Position = new Vector2(playerObj.X, playerObj.Y);

            foreach (var mobSpawn in instanceLayer.Objects.ToLookup(t => t.Type)["mobSpawn"])
            {
                var mobType = mobSpawn.Properties["mobType"];
                var mobEntity = Mob.MakeMobEntity(mobSpawn.Name != "" ? mobSpawn.Name : "mob", mobType);
                mobEntity.Position = new Vector2(mobSpawn.X, mobSpawn.Y);
            }

            var triggerLayer = Map.GetObjectGroup("triggers");
            var triggersByType = triggerLayer.Objects.ToLookup(t => t.Type);

            foreach (var exit in triggersByType["exit"])
            {
                var exitEntity = CreateEntity(exit.Name != "" ? exit.Name : "exit");
                var exitCollider = exitEntity.AddComponent(new BoxCollider(exit.X, exit.Y, exit.Width, exit.Height));
                exitCollider.IsTrigger = true;
                var mapSrc = Path.GetFileName(exit.Properties["map"]);
                var spawn = exit.Properties["spawn"];
                exitEntity.AddComponent(new ExitTrigger(mapSrc, spawn));
            }

            var cameraEntity = Camera.Entity;
            cameraEntity.AddComponent(new FollowCamera(playerEntity));
            cameraEntity.AddComponent(new CameraComponent(
                new Vector2(Map.TileWidth, 0),
                new Vector2(Map.TileWidth * (Map.Width - 1), Map.TileHeight * Map.Height)));
            cameraEntity.Position = playerEntity.Position;
        }
    }
}
