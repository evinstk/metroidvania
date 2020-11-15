﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Game.Tiled
{
    class Tileset
    {
        public string Image;
        public Tile[] Tiles;
        public int TileWidth;
        public int TileHeight;

        static Dictionary<string, Tileset> _tilesets = new Dictionary<string, Tileset>();

        public static Tileset Load(string path)
        {
            if (_tilesets.TryGetValue(path, out var tileset))
                return tileset;

            using (var stream = TitleContainer.OpenStream(path))
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                tileset = new JsonSerializer().Deserialize<Tileset>(jsonTextReader);
                _tilesets[path] = tileset;
                return tileset;
            }
        }
    }

    class Tile
    {
        public int Id;
        public string Type;
        public AnimationFrame[] Animation;
        public ObjectGroup ObjectGroup;
    }

    class AnimationFrame
    {
        public int Duration;
        public int TileId;
    }

    class ObjectGroup
    {
        public TiledObject[] Objects;
    }

    class TiledObject
    {
        public string Type;
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
}