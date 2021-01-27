using Game.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Game
{
    class MapRenderer : RenderableComponent
    {
        public override float Width => _width * _tileWidth;
        public override float Height => _height * _tileHeight;

        int _tileWidth;
        int _tileHeight;
        int _width;
        int _height;

        Tile[] _tiles;

        public MapRenderer(RoomData roomData, int layerIndex)
        {
            _tileWidth = roomData.TileWidth;
            _tileHeight = roomData.TileHeight;
            _width = roomData.Width;
            _height = roomData.Height;
            _tiles = new Tile[_width * _height];

            var layer = roomData.Layers[layerIndex];
            foreach (var tile in layer.Tiles)
            {
                var loc = tile.LayerLocation;
                // location may be outside defined bounds
                if (loc.X < _width && loc.Y < _height)
                {
                    _tiles[loc.X + loc.Y * _width] = new Tile
                    {
                        Texture = Core.Scene.Content.LoadTexture("Content/Textures/" + tile.Tileset),
                        SourceRect = new RectangleF(
                            // TODO: use tileset tile size
                            tile.TilesetLocation.ToVector2() * new Vector2(16, 16),
                            new Vector2(16, 16)),
                    };
                }
            }
        }

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

        class Tile
        {
            public Texture2D Texture;
            public RectangleF SourceRect;
        }
    }
}
