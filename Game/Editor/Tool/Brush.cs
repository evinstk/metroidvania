using Nez;
using System.IO;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class Brush
        {
            ToolWindow _window;

            public Brush(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var roomData = _window._roomWindow.RoomData;
                var textureFile = _window._tilesetWindow.TextureFile;

                if (Input.LeftMouseButtonDown
                    && roomData != null
                    && textureFile != null
                    && Core.Scene.Camera.Bounds.Contains(Input.MousePosition))
                {
                    var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                    var layerPoint = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                    if (layerPoint.X < 0 || layerPoint.X >= roomData.Width || layerPoint.Y < 0 || layerPoint.Y >= roomData.Height)
                        return;

                    var tileSelection = _window._tilesetWindow.TileSelection;

                    // TODO: select layer through window
                    var layer = roomData.Layers[0];

                    LayerTile existingTile = null;
                    foreach (var tile in layer.Tiles)
                    {
                        if (tile.LayerLocation == layerPoint)
                            existingTile = tile;
                    }

                    if (existingTile != null)
                        layer.Tiles.Remove(existingTile);

                    layer.Tiles.Add(new LayerTile
                    {
                        Tileset = Path.GetFileName(textureFile),
                        TilesetLocation = tileSelection,
                        LayerLocation = layerPoint,
                    });
                }
            }
        }
    }
}
