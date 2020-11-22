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
        public string MapSrc { get; }
        public string Spawn { get; }
        public int StartingHealth { get; }

        static int resWidth = 1920;
        static int resHeight = 1080;

        public TmxMap Map { get; private set; }

        public MainScene(
            string mapSrc,
            string spawn,
            int startingHealth = -1)
        {
            MapSrc = mapSrc;
            Spawn = spawn;
            StartingHealth = startingHealth;
        }

        public override void OnStart()
        {
            SetDesignResolution(resWidth, resHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(resWidth, resHeight);

            Physics.RaycastsHitTriggers = true;

            CreateEntity("gameManager").AddComponent<GameManager>();

            Map = Content.LoadTiledMap("Content/Maps/" + MapSrc);

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

            var playerObj = instanceLayer.Objects.First(o => o.Name == Spawn && o.Type == "playerSpawn");
            var playerEntity = Mob.MakeMobEntity("player", "Hero", new MobOptions
            {
                PlayerControlled = true,
                StartingHealth = StartingHealth,
                Team = Teams.A,
            });
            playerEntity.Position = new Vector2(playerObj.X, playerObj.Y);

            var triggerLayer = Map.GetObjectGroup("triggers");
            if (triggerLayer != null)
            {
                foreach (var trigger in triggerLayer.Objects)
                {
                    var triggerEntity = CreateEntity(trigger.Name != string.Empty ? trigger.Name : "trigger" + trigger.Id.ToString());
                    var triggerCollider = triggerEntity.AddComponent(new BoxCollider(trigger.X, trigger.Y, trigger.Width, trigger.Height));
                    triggerCollider.IsTrigger = true;
                    Flags.SetFlagExclusive(ref triggerCollider.CollidesWithLayers, Layer.Default);
                    if (trigger.Type == "exit")
                    {
                        var mapSrc = Path.GetFileName(trigger.Properties["map"]);
                        var spawn = trigger.Properties["spawn"];
                        triggerEntity.AddComponent(new ExitTrigger(mapSrc, spawn));
                    }
                }
            }

            var cameraEntity = Camera.Entity;
            var followCamera = playerEntity.AddComponent(new FollowCamera(playerEntity, Camera, FollowCamera.CameraStyle.CameraWindow));
            followCamera.MapLockEnabled = true;
            followCamera.MapSize = new Vector2(Map.TileWidth * Map.Width, Map.TileHeight * Map.Height);
            cameraEntity.Position = playerEntity.Position;

            if (Map.Properties.TryGetValue("script", out var scriptSrc))
            {
                var mapScriptEntity = CreateEntity("mapScript");
                mapScriptEntity.AddComponent(new Scripting.MapScript(Path.GetFileName(scriptSrc)));
            }
        }
    }
}
