using Game.Editor.Prefab;
using Nez;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class Prefab
        {
            ToolWindow _window;
            public Prefab(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var selectedEntity = _window._entityWindow.SelectedEntity;
                var roomData = _window._roomWindow.RoomData;

                if (Input.LeftMouseButtonPressed
                    && selectedEntity != null
                    && roomData != null
                    && Core.Scene.Camera.Bounds.Contains(Input.MousePosition))
                {
                    var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                    roomData.Entities.Add(new RoomEntity
                    {
                        Name = selectedEntity.Name,
                        Position = worldPoint,
                    });
                }
            }

            public void Render(Batcher batcher, Camera camera)
            {
                var selectedEntity = _window._entityWindow.SelectedEntity;
                var roomData = _window._roomWindow.RoomData;

                if (selectedEntity != null
                    && roomData != null)
                {
                    var spriteData = selectedEntity.Components.Find(c => c is SpriteData) as SpriteData;
                    var texture = spriteData?.Texture;
                    if (texture == null)
                        return;

                    var mousePos = Input.MousePosition;
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
