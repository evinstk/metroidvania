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
                var roomData = _window._roomWindow.RoomData;

                if (Input.LeftMouseButtonDown
                    && roomData != null
                    && Core.Scene.Camera.Bounds.Contains(Input.MousePosition))
                {
                    var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                    var layerPoint = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                    if (layerPoint.X < 0 || layerPoint.X >= roomData.Width || layerPoint.Y < 0 || layerPoint.Y >= roomData.Height)
                        return;

                    // TODO: select layer through window
                    var layer = roomData.Layers[0];

                    layer.Tiles.RemoveAll(t => t.LayerLocation == layerPoint);
                }
            }
        }
    }
}
