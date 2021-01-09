using Nez;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class PrefabTool
        {
            ToolWindow _window;
            public PrefabTool(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var selectedPrefab = EditorState.SelectedPrefab;
                var roomData = EditorState.RoomData;

                var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                if (Input.LeftMouseButtonPressed
                    && selectedPrefab != null
                    && roomData != null
                    && Core.Scene.Camera.Bounds.Contains(worldPoint))
                {
                    worldPoint.X = Mathf.RoundToNearest(worldPoint.X, 4);
                    worldPoint.Y = Mathf.RoundToNearest(worldPoint.Y, 4);
                    roomData.Entities.Add(new RoomEntity
                    {
                        Name = selectedPrefab.Name,
                        PrefabId = selectedPrefab.Id,
                        Position = worldPoint,
                    });
                }
            }

            public void Render(Batcher batcher, Camera camera)
            {
                var selectedPrefab = EditorState.SelectedPrefab;
                var roomData = EditorState.RoomData;

                if (selectedPrefab != null
                    && roomData != null)
                {
                    var mousePos = Core.Scene.Camera.MouseToWorldPoint();
                    mousePos.X = Mathf.RoundToNearest(mousePos.X, 4);
                    mousePos.Y = Mathf.RoundToNearest(mousePos.Y, 4);
                    selectedPrefab.Render(batcher, mousePos);
                }
            }
        }
    }
}
