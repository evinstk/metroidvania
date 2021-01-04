﻿using Nez;

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
                    var layerPoint = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                    if (layerPoint.X < 0 || layerPoint.X >= roomData.Width || layerPoint.Y < 0 || layerPoint.Y >= roomData.Height)
                        return;

                    layer.Tiles.RemoveAll(t => t.LayerLocation == layerPoint);
                }
            }
        }
    }
}
