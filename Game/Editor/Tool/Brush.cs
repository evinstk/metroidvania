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

                    var tileSelection = EditorState.TileSelection;

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
