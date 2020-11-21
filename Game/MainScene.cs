using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using System;
using System.IO;
using System.Linq;

namespace Game
{
	class MainScene : Scene
	{
        string _mapSrc;
        string _spawn;
        int _startingHealth;

        static int resWidth = 1920;
        static int resHeight = 1080;

        public TmxMap Map { get; private set; }

        public MainScene(
            string mapSrc,
            string spawn,
            int startingHealth = -1)
        {
            _mapSrc = mapSrc;
            _spawn = spawn;
            _startingHealth = startingHealth;
        }

        public override void OnStart()
        {
            SetDesignResolution(resWidth, resHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(resWidth, resHeight);

            Physics.RaycastsHitTriggers = true;

            Map = Content.LoadTiledMap("Content/Maps/" + _mapSrc);

            var mapEntity = CreateEntity("map");
            mapEntity.AddComponent(new TiledMapRenderer(Map, "terrain"));

            var instanceLayer = Map.GetObjectGroup("instances");

            foreach (var mobSpawn in instanceLayer.Objects.ToLookup(t => t.Type)["mobSpawn"])
            {
                var mobType = mobSpawn.Properties["mobType"];
                if (!mobSpawn.Properties.TryGetValue("color", out var colorStr))
                    colorStr = "#ffffffff";
                colorStr = "0x" + colorStr.Substring(1, 6) + "ff";
                var color = new Color(Convert.ToUInt32(colorStr, 16));
                color.A = 255;
                if (!mobSpawn.Properties.TryGetValue("team", out var team))
                {
                    team = "1";
                }
                mobSpawn.Properties.TryGetValue("dialog", out var dialogSrc);
                if (dialogSrc != null) dialogSrc = Path.GetFileName(dialogSrc);
                var mobEntity = Mob.MakeMobEntity(mobSpawn.Name != "" ? mobSpawn.Name : "mob", mobType, new MobOptions
                {
                    Color = color,
                    Team = (Teams)int.Parse(team),
                    DialogSrc = dialogSrc,
                });
                mobEntity.Position = new Vector2(mobSpawn.X, mobSpawn.Y);
            }

            var playerObj = instanceLayer.Objects.First(o => o.Name == _spawn && o.Type == "playerSpawn");
            var playerEntity = Mob.MakeMobEntity("player", "Hero", new MobOptions
            {
                PlayerControlled = true,
                StartingHealth = _startingHealth,
                Team = Teams.A,
            });
            playerEntity.Position = new Vector2(playerObj.X, playerObj.Y);

            var triggerLayer = Map.GetObjectGroup("triggers");
            var triggersByType = triggerLayer.Objects.ToLookup(t => t.Type);

            foreach (var exit in triggersByType["exit"])
            {
                var exitEntity = CreateEntity(exit.Name != "" ? exit.Name : "exit");
                var exitCollider = exitEntity.AddComponent(new BoxCollider(exit.X, exit.Y, exit.Width, exit.Height));
                exitCollider.IsTrigger = true;
                Flags.SetFlagExclusive(ref exitCollider.CollidesWithLayers, Layer.Default);
                var mapSrc = Path.GetFileName(exit.Properties["map"]);
                var spawn = exit.Properties["spawn"];
                exitEntity.AddComponent(new ExitTrigger(mapSrc, spawn));
            }

            var cameraEntity = Camera.Entity;
            var followCamera = playerEntity.AddComponent(new FollowCamera(playerEntity, Camera, FollowCamera.CameraStyle.CameraWindow));
            followCamera.MapLockEnabled = true;
            followCamera.MapSize = new Vector2(Map.TileWidth * Map.Width, Map.TileHeight * Map.Height);
            cameraEntity.Position = playerEntity.Position;
        }
    }
}
