using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Persistence;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    class OgmoProject
    {
        public List<OgmoTileset> tilesets;
    }

    class OgmoTileset
    {
        public string label;
        public string path;
        public int tileWidth;
        public int tileHeight;
    }

    class OgmoLevel
    {
        public int width;
        public int height;
        public List<OgmoLevelLayer> layers;
        public Dictionary<string, string> values;
    }

    class OgmoLevelLayer
    {
        public string name;
        public int gridCellWidth;
        public int gridCellHeight;
        public int gridCellsX;
        public int gridCellsY;
        public List<int> data;
        public List<OgmoEntity> entities;
    }

    class OgmoEntity
    {
        public string name;
        public int x;
        public int y;
        public Dictionary<string, string> values;
    }

    class MapCollider :
#if DEBUG
        RenderableComponent
#else
        Component
#endif
    {
        OgmoLevelLayer _layer;
        Collider[] _colliders;

        public MapCollider(OgmoLevelLayer layer, int width, int height)
        {
            _layer = layer;
            _width = width;
            _height = height;
        }

        public override void OnAddedToEntity()
        {
            List<Rectangle> rects = new List<Rectangle>();
            for (var i = 0; i < _layer.data.Count; ++i)
            {
                var tile = _layer.data[i];
                if (tile != -1)
                {
                    var x = i % _layer.gridCellsX * _layer.gridCellWidth;
                    var y = i / _layer.gridCellsX * _layer.gridCellHeight;
                    rects.Add(new Rectangle(x, y, _layer.gridCellWidth, _layer.gridCellHeight));
                }
            }

            _colliders = new Collider[rects.Count];
            for (var i = 0; i < rects.Count; ++i)
            {
                var collider = new BoxCollider(rects[i]);
                collider.PhysicsLayer = Mask.Terrain;
                collider.Entity = Entity;
                _colliders[i] = collider;
                Physics.AddCollider(collider);
            }
        }

#if DEBUG
        public override float Width => _width;
        float _width = 1;
        public override float Height => _height;
        float _height = 1;

        public override void Render(Batcher batcher, Camera camera)
        {
        }

        public override void DebugRender(Batcher batcher)
        {
            foreach (var collider in _colliders)
                collider.DebugRender(batcher);
        }
#endif
    }

    class MapRenderer : RenderableComponent
    {
        class Tile
        {
            public Texture2D Texture;
            public RectangleF SourceRect;
        }

        OgmoProject _project;
        OgmoLevel _level;
        int _layerIndex;
        int _width;
        int _height;
        int _tileWidth;
        int _tileHeight;

        Tile[] _tiles;

        public MapRenderer(OgmoProject project, OgmoLevel level, int layerIndex)
        {
            _project = project;
            _level = level;
            _layerIndex = layerIndex;

            var layer = _level.layers[_layerIndex];
            _tileWidth = layer.gridCellWidth;
            _tileHeight = layer.gridCellHeight;
            _width = layer.gridCellsX;
            _height = layer.gridCellsY;

            _tiles = new Tile[_width * _height];
            for (var i = 0; i < layer.data.Count; ++i)
            {
                var tile = layer.data[i];
                if (tile != -1)
                {
                    var tileset = FindTileset(tile);
                    var texture = Core.Scene.Content.LoadTexture(
                        ContentPath.Tilesets + Path.GetFileName(tileset.path));
                    var tx = tile % (texture.Width / tileset.tileWidth) * tileset.tileWidth;
                    var ty = tile / (texture.Width / tileset.tileWidth) * tileset.tileHeight;
                    _tiles[i] = new Tile
                    {
                        SourceRect = new RectangleF(
                            // TODO: use tileset tile size
                            tx, ty, layer.gridCellWidth, layer.gridCellHeight),
                        Texture = texture,
                    };
                }
            }
        }

        // TODO: implement correctly
        OgmoTileset FindTileset(int index) => _project.tilesets[0];

        public override float Width => _width * _tileWidth;
        public override float Height => _height * _tileHeight;

        public override void Render(Batcher batcher, Camera camera)
        {
            var cameraClipBounds = camera.Bounds;
            cameraClipBounds.Location -= Entity.Position + _localOffset;

            var scale = Transform.Scale;
            var tileWidth = _tileWidth * scale.X;
            var tileHeight = _tileHeight * scale.Y;
            var tileSize = new Vector2(tileWidth, tileHeight);

            int minX, minY, maxX, maxY;
            minX = WorldToTilePositionX(cameraClipBounds.Left);
            minY = WorldToTilePositionY(cameraClipBounds.Top);
            maxX = WorldToTilePositionX(cameraClipBounds.Right);
            maxY = WorldToTilePositionY(cameraClipBounds.Bottom);

            for (var y = minY; y <= maxY; ++y)
            {
                for (var x = minX; x <= maxX; ++x)
                {
                    var tile = _tiles[x + y * _width];
                    if (tile != null)
                        batcher.Draw(
                            tile.Texture,
                            new RectangleF(
                                new Vector2(x * tileWidth, y * tileHeight) + Entity.Position + _localOffset,
                                tileSize),
                            tile.SourceRect,
                            Color);
                }
            }
        }

        int WorldToTilePositionX(float x)
        {
            var tileX = Mathf.FastFloorToInt(x / _tileWidth);
            return Mathf.Clamp(tileX, 0, _width - 1);
        }

        int WorldToTilePositionY(float y)
        {
            var tileY = Mathf.FloorToInt(y / _tileHeight);
            return Mathf.Clamp(tileY, 0, _height - 1);
        }
    }
}
