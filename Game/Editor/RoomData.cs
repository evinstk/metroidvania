using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Editor
{
    class RoomMetadata
    {
        public RoomData RoomData;
        public string Filename;
    }

    [Serializable]
    class RoomData
    {
        public string Name = "";

        public int Width = 128;
        public int Height = 128;
        public int TileWidth = 16;
        public int TileHeight = 16;
        public Point TileSize => new Point(TileWidth, TileHeight);

        public List<RoomLayer> Layers = new List<RoomLayer>
        {
            new RoomLayer { Name = "terrain", HasColliders = true },
            new RoomLayer { Name = "doodad" },
        };

        public void SaveToFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            var roomData = Json.ToJson(this, new JsonSettings
            {
                PrettyPrint = true,
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = true,
            });

            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine(roomData);
            }
        }

        public static RoomData ReadFromFile(string filename)
        {
            var roomDataStr = File.ReadAllText(filename);
            var roomData = Json.FromJson<RoomData>(roomDataStr);
            return roomData;
        }
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
}
