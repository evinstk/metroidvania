using Game.Entities;
using Game.Scripts;
using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Worlds
{
    class WorldLoader : SceneComponent
    {
        public class LayerConfig
        {
            public int RenderLayer;
            public int CollisionMask;
        }

        public class WorldProperties
        {
            public Dictionary<string, LayerConfig> Layers = new Dictionary<string, LayerConfig>();
        }

        class RoomBounds
        {
            public Rectangle Bounds;
            public Room Room;
            public OgmoLevel Level;
        }


        OgmoProject _project;
        string _projectRoot;
        World _world;
        WorldProperties _properties;
        List<RoomBounds> _bounds = new List<RoomBounds>();

        public void LoadWorld(
            string ogmoProject,
            string worldName,
            WorldProperties properties = null)
        {
            _project = GameContent.LoadOgmoProject(ogmoProject);
            _projectRoot = Path.GetDirectoryName(ogmoProject);
            _world = GameContent.LoadWorld(worldName);
            _properties = properties;

            foreach (var room in _world.Rooms)
            {
                var roomPath = $"{worldName}/{room.MapName}";
                var level = GameContent.LoadOgmoLevel(roomPath);
                var rect = new Rectangle(
                    room.Position.X, room.Position.Y,
                    level.width, level.height);
                _bounds.Add(new RoomBounds
                {
                    Bounds = rect,
                    Room = room,
                    Level = level,
                });
            }
        }

        public void RunRoom(Vector2 location)
        {
            if (_world == null)
                throw new Exception("No world loaded.");

            var roomBounds = _bounds.Find(r => r.Bounds.Contains(location));
            if (roomBounds != null)
            {
                _bounds.Remove(roomBounds);

                var entityLoader = Scene.GetSceneComponent<EntityLoader>();

                var map = Scene.CreateEntity("map");
                map.Position = roomBounds.Bounds.Location.ToVector2();
                var level = roomBounds.Level;
                for (var i = 0; i < level.layers.Count; ++i)
                {
                    var layer = level.layers[i];
                    LayerConfig layerConfig = null;
                    _properties?.Layers.TryGetValue(layer.name, out layerConfig);
                    if (layer.data != null)
                    {
                        var renderer = map.AddComponent(new MapRenderer(_project, level, i));
                        renderer.RenderLayer = layerConfig?.RenderLayer ?? 0;

                        if (layerConfig?.CollisionMask > 0)
                        {
                            var collider = map.AddComponent(new MapCollider(layer));
                            collider.PhysicsLayer = layerConfig.CollisionMask;
                        }
                    }
                    else if (layer.entities != null)
                    {
                        foreach (var entity in layer.entities)
                        {
                            var pos = new Vector2(entity.x, entity.y) + roomBounds.Bounds.Location.ToVector2();
                            entityLoader.Create(entity, pos);
                        }
                    }
                }

                string script = null;
                if (level.values?.TryGetValue("script", out script) == true && script != "proj:")
                {
                    var scriptPath = $"{_projectRoot}/{script.Substring(5)}";
                    Scene.GetSceneComponent<ScriptLoader>().LoadScript(scriptPath);
                }
            }
        }
    }
}
