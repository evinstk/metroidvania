using Game.Entities;
using Game.Worlds;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Nez;
using System.Collections.Generic;
using static Game.Worlds.WorldLoader;

namespace Game.Scripts
{
    class ScreenProxy
    {
        public void SetSize(int width, int height) => Screen.SetSize(width, height);
    }

    class PrototypeSceneProxy
    {
        Scene _scene;

        public PrototypeSceneProxy(Scene scene)
        {
            _scene = scene;
        }

        public void SetDesignResolution(int width, int height) => _scene.SetDesignResolution(width, height, Scene.SceneResolutionPolicy.ShowAllPixelPerfect);
        public void LoadWorld(string projectName, string worldName, Dictionary<string, DynValue> props)
        {
            var properties = new WorldProperties();
            if (props.TryGetValue("layers", out var layersDynValue))
            {
                var layers = layersDynValue.ToObject<Dictionary<string, Dictionary<string, object>>>();
                foreach (var layer in layers)
                {
                    var layerConfig = new LayerConfig();
                    if (layer.Value.TryGetValue("render_layer", out var renderLayer))
                        layerConfig.RenderLayer = (int)(double)renderLayer;
                    if (layer.Value.TryGetValue("collision_mask", out var collisionMask))
                        layerConfig.CollisionMask = (int)(double)collisionMask;
                    properties.Layers[layer.Key] = layerConfig;
                }
            }

            _scene.GetSceneComponent<WorldLoader>().LoadWorld(projectName, worldName, properties);
        }
        public void RunRoom(Vector2 location) => _scene.GetSceneComponent<WorldLoader>().RunRoom(location);

        public Entity Instantiate(string entityType, Vector2 position) => _scene.GetSceneComponent<EntityLoader>().Create(new OgmoEntity { name = entityType }, position);
    }

    class EntityProxy
    {
        Entity _entity;

        public EntityProxy(Entity entity)
        {
            _entity = entity;
        }

        public string GetName() => _entity.Name;
        public Vector2 GetPosition() => _entity.Position;
    }
}
