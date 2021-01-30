using Nez;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class Erase
        {
            ToolWindow _window;

            public Erase(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var roomData = EditorState.RoomData;
                var layer = EditorState.SelectedLayer;

                var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                if (Input.LeftMouseButtonDown
                    && roomData != null
                    && layer != null
                    && Core.Scene.Camera.Bounds.Contains(worldPoint))
                {
                    if (worldPoint.X < 0) worldPoint.X -= roomData.TileSize.X;
                    if (worldPoint.Y < 0) worldPoint.Y -= roomData.TileSize.Y;
                    var layerPoint = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                    layer.Tiles.RemoveAll(t => t.LayerLocation == layerPoint);
                }
            }
        }
    }
}
