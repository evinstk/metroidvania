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
        public List<PrefabComponent> Components = new List<PrefabComponent>();
        public RoomData Room { get; set; }
    }
}
