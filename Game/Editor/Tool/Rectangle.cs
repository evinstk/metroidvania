using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class Rectangle
        {
            ToolWindow _window;
            Point _start;
            bool _down;
            List<LayerTile> _toRemove = new List<LayerTile>();

            public Rectangle(ToolWindow window)
            {
            }

            public void Update()
            {
                var roomData = EditorState.RoomData;
                var textureFile = EditorState.TilesetTextureFile;
                var layer = EditorState.SelectedLayer;
                var tileSelections = EditorState.TileSelections;

                if (roomData == null || textureFile == null || layer == null || tileSelections.Width == 0 || tileSelections.Height == 0)
                    return;

                var worldPoint = Core.Scene.Camera.MouseToWorldPoint();

                if (Input.LeftMouseButtonPressed && Core.Scene.Camera.Bounds.Contains(worldPoint))
                {
                    if (worldPoint.X < 0) worldPoint.X -= roomData.TileSize.X;
                    if (worldPoint.Y < 0) worldPoint.Y -= roomData.TileSize.Y;
                    _start = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();
                    _down = true;
                }

                if (Input.LeftMouseButtonReleased && _down)
                {
                    _down = false;
                    if (worldPoint.X < 0) worldPoint.X -= roomData.TileSize.X;
                    if (worldPoint.Y < 0) worldPoint.Y -= roomData.TileSize.Y;
                    var end = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                    var minX = Math.Min(_start.X, end.X);
                    var minY = Math.Min(_start.Y, end.Y);
                    var width = Math.Abs(_start.X - end.X);
                    var height = Math.Abs(_start.Y - end.Y);

                    for (var x = 0; x < width + 1; ++x)
                    {
                        for (var y = 0; y < height + 1; ++y)
                        {
                            var offset = new Point(x, y);
                            var point = new Point(minX, minY) + offset;
                            foreach (var tile in layer.Tiles)
                            {
                                if (tile.LayerLocation == point)
                                    _toRemove.Add(tile);
                            }
                            var selectionOffset = offset;
                            selectionOffset.X %= tileSelections.Width;
                            selectionOffset.Y %= tileSelections.Height;
                            var selection = tileSelections.Location + selectionOffset;
                            layer.Tiles.Add(new LayerTile
                            {
                                Tileset = Path.GetFileName(textureFile),
                                TilesetLocation = selection,
                                LayerLocation = point,
                            });
                        }
                    }
                    foreach (var tile in _toRemove)
                        layer.Tiles.Remove(tile);
                    _toRemove.Clear();
                }
            }
        }
    }
}
