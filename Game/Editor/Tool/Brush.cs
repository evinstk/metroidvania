using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;
using System.IO;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class Brush
        {
            ToolWindow _window;
            List<LayerTile> _toRemove = new List<LayerTile>();

            public Brush(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var roomData = EditorState.RoomData;
                var textureFile = EditorState.TilesetTextureFile;
                var layer = EditorState.SelectedLayer;

                var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                if (Input.LeftMouseButtonDown
                    && roomData != null
                    && textureFile != null
                    && layer != null
                    && Core.Scene.Camera.Bounds.Contains(worldPoint))
                {
                    if (worldPoint.X < 0) worldPoint.X -= roomData.TileSize.X;
                    if (worldPoint.Y < 0) worldPoint.Y -= roomData.TileSize.Y;
                    var layerPoint = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                    var tileSelections = EditorState.TileSelections;

                    foreach (var tile in layer.Tiles)
                    {
                        for (var x = 0; x < tileSelections.Width; ++x)
                        {
                            for (var y = 0; y < tileSelections.Height; ++y)
                            {
                                var point = layerPoint + new Point(x, y);
                                if (tile.LayerLocation == point)
                                    _toRemove.Add(tile);
                            }
                        }
                    }

                    foreach (var tile in _toRemove)
                        layer.Tiles.Remove(tile);
                    _toRemove.Clear();

                    for (var x = 0; x < tileSelections.Width; ++x)
                    {
                        for (var y = 0; y < tileSelections.Height; ++y)
                        {
                            var offset = new Point(x, y);
                            var selection = tileSelections.Location + offset;
                            var point = layerPoint + offset;
                            layer.Tiles.Add(new LayerTile
                            {
                                Tileset = Path.GetFileName(textureFile),
                                TilesetLocation = selection,
                                LayerLocation = point,
                            });
                        }
                    }
                }
            }
        }
    }
}
