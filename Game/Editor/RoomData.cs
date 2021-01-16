using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Game.Editor
{
    [Serializable]
    class RoomData : IResource
    {
        [JsonInclude]
        public string Id { get; set; } = Utils.RandomString();
        public string DisplayName => Name;
        public string Name = "New Room";

        public int Width = 128;
        public int Height = 128;
        public int TileWidth = 16;
        public int TileHeight = 16;
        public Point TileSize => new Point(TileWidth, TileHeight);
        public int LightRendererClearColor = 127;
        public string Script = null;

        public List<RoomLayer> Layers = new List<RoomLayer>
        {
            new RoomLayer { Name = "terrain", HasColliders = true },
            new RoomLayer { Name = "doodad" },
        };
        public List<RoomEntity> Entities = new List<RoomEntity>();
        public RoomVariables RoomVariables = new RoomVariables();

        public void AddEntity(RoomEntity entity)
        {
            Entities.Add(entity);
            entity.Room = this;
            entity.Initialize();
        }

        public void Initialize()
        {
            // set rooms for all entities before calling Initialize
            foreach (var entity in Entities)
                entity.Room = this;
            foreach (var entity in Entities)
                entity.Initialize();
        }
    }

    // separate class to avoid inpsector errors
    class RoomVariables
    {
        public List<RoomVariable> Variables = new List<RoomVariable>();
    }

    class RoomLayer
    {
        public string Name = "New Layer";
        public List<LayerTile> Tiles = new List<LayerTile>();
        public bool HasColliders = false;
    }

    class LayerTile
    {
        public string Tileset;
        public Point TilesetLocation;
        public Point LayerLocation;
    }

    class RoomEntity
    {
        static readonly Type entityOnlyComponentType = typeof(EntityOnlyComponent);

        public string Id = Utils.RandomString();
        public string Name;
        public string PrefabId;
        public Vector2 Position;

        public PrefabData Prefab => Core.GetGlobalManager<PrefabManager>().GetResource(PrefabId);
        public List<EntityOnlyComponent> EntityOnlyComponents = new List<EntityOnlyComponent>();
        public List<DataComponent> Components = new List<DataComponent>();
        public RoomData Room { get; set; }

        public void Initialize()
        {
            SyncEntityOnlyComponents();
        }

        public void SyncEntityOnlyComponents()
        {
            var components = new List<DataComponent>();

            var prefab = Prefab;
            if (prefab != null)
                components.AddRange(prefab.Components);
            components.AddRange(Components);

            foreach (var component in components)
            {
                // component already exists for this entity
                if (EntityOnlyComponents.FindIndex(c => c.DataComponentId == component.Id) > -1)
                    continue;

                var entityOnlyType = component.GetType().GetAttribute<EntityOnlyAttribute>();
                if (entityOnlyType != null)
                {
                    if (entityOnlyType.EntityOnlyComponentType.GetTypeInfo().IsSubclassOf(entityOnlyComponentType))
                    {
                        var eoComponent = (EntityOnlyComponent)Activator.CreateInstance(entityOnlyType.EntityOnlyComponentType);
                        eoComponent.DataComponentId = component.Id;
                        EntityOnlyComponents.Add(eoComponent);
                    }
                    else
                    {
                        Debug.Warn($"found entity only {entityOnlyType.EntityOnlyComponentType} but it is not a subclass of EntityOnlyComponent");
                    }
                }
            }

            var staleComponents = new List<EntityOnlyComponent>();
            foreach (var eoComponent in EntityOnlyComponents)
            {
                if (components.FindIndex(c => c.Id == eoComponent.DataComponentId) == -1)
                    staleComponents.Add(eoComponent);
            }
            foreach (var eoComponent in staleComponents)
                EntityOnlyComponents.Remove(eoComponent);
        }

        public void Render(Batcher batcher)
        {
            Prefab?.Render(batcher, Position);
            foreach (var component in Components)
                component.Render(batcher, Position);
        }

        public bool Select(Vector2 mousePosition)
        {
            if (Prefab?.Select(Position, mousePosition) == true)
                return true;
            foreach (var component in Components)
            {
                if (component.Select(Position, mousePosition))
                    return true;
            }
            return false;
        }

        public Entity CreateEntity(Scene scene, Vector2 offset = new Vector2())
        {
            var entity = scene.CreateEntity(Name);
            entity.SetPosition(Position + offset);
            var roomEntityData = entity.AddComponent<RoomEntityComponent>();
            roomEntityData.RoomEntityId = Id;
            roomEntityData.RoomId = Room.Id;
            var prefab = Prefab;
            if (prefab != null)
            {
                foreach (var component in prefab.Components)
                    component.AddToEntity(entity);
            }
            foreach (var component in Components)
                component.AddToEntity(entity);
            foreach (var eoComponent in EntityOnlyComponents)
                eoComponent.AddToEntity(entity);
            return entity;
        }
    }

    class RoomEntityComponent : Component
    {
        //public RoomEntity RoomEntity;
        public string RoomEntityId;
        public string RoomId;
    }
}
