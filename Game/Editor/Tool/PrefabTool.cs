using Game.Editor.Prefab;
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

                if (Input.LeftMouseButtonPressed
                    && selectedPrefab != null
                    && roomData != null
                    && Core.Scene.Camera.Bounds.Contains(Input.MousePosition))
                {
                    var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
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
                var selectedEntity = EditorState.SelectedPrefab;
                var roomData = EditorState.RoomData;

                if (selectedEntity != null
                    && roomData != null)
                {
                    var spriteData = selectedEntity.Components.Find(c => c is SpriteData) as SpriteData;
                    var texture = spriteData?.TextureData.Texture;
                    if (texture == null)
                        return;

                    var mousePos = Core.Scene.Camera.MouseToWorldPoint();
                    mousePos.X = Mathf.RoundToNearest(mousePos.X, 4);
                    mousePos.Y = Mathf.RoundToNearest(mousePos.Y, 4);
                    batcher.Draw(
                        texture,
                        mousePos - spriteData.Origin,
                        spriteData.SourceRect,
                        spriteData.Color);
                }
            }
        }
    }
}
