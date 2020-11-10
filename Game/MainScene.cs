﻿using Game;
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
        string _spawn;

        public MainScene(string mapSrc, string spawn)
        {
            _mapSrc = mapSrc;
            _spawn = spawn;
        }

        public override void OnStart()
        {
            SetDesignResolution(1280, 768, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1280, 768);

            var map = Content.LoadTiledMap("Content/Maps/" + _mapSrc);

            var mapEntity = CreateEntity("map");
            mapEntity.AddComponent(new TiledMapRenderer(map, "terrain"));

            var instanceLayer = map.GetObjectGroup("instances");

            var playerObj = instanceLayer.Objects.First(o => o.Name == _spawn && o.Type == "playerSpawn");
            var playerEntity = CreateEntity("player");
            playerEntity.Position = new Vector2(playerObj.X, playerObj.Y);
            playerEntity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("terrain")));
            playerEntity.AddComponent<ControllerComponent>();
            playerEntity.AddComponent(new AnimationComponent("Warrior"));
            var playerCollider = playerEntity.AddComponent<BoxCollider>();

            var triggerLayer = map.GetObjectGroup("triggers");
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
                new Vector2(map.TileWidth, 0),
                new Vector2(map.TileWidth * (map.Width - 1), map.TileHeight * map.Height)));
            cameraEntity.Position = playerEntity.Position;
        }
    }
}
