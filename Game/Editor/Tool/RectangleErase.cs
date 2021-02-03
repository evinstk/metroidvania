using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class RectangleErase
        {
            ToolWindow _window;
            Point _start;
            bool _down;
            List<LayerTile> _toRemove = new List<LayerTile>();

            public RectangleErase(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var roomData = EditorState.RoomData;
                var textureFile = EditorState.TilesetTextureFile;
                var layer = EditorState.SelectedLayer;

                if (roomData == null || textureFile == null || layer == null)
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

                    foreach (var tile in layer.Tiles)
                    {
                        var location = tile.LayerLocation;
                        if (location.X >= minX
                            && location.X <= minX + width
                            && location.Y >= minY
                            && location.Y <= minY + height)
                            _toRemove.Add(tile);
                    }
                    foreach (var tile in _toRemove)
                        layer.Tiles.Remove(tile);
                    _toRemove.Clear();
                }
            }
        }
    }
}
