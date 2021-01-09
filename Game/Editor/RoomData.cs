using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System;
using System.Collections.Generic;

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
        public string Id = Utils.RandomString();
        public string Name;
        public string PrefabId;
        public Vector2 Position;

        public PrefabData Prefab => Core.GetGlobalManager<PrefabManager>().GetResource(PrefabId);
        public List<DataComponent> Components = new List<DataComponent>();
        public RoomData Room { get; set; }

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
    }
}
