using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class SelectTool
        {
            ToolWindow _window;

            public SelectTool(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var roomData = EditorState.RoomData;

                if (Input.LeftMouseButtonPressed
                    && roomData != null)
                {
                    var position = Core.Scene.Camera.MouseToWorldPoint();
                    foreach (var entity in roomData.Entities)
                    {
                        var prefab = entity.Prefab;
                        var spriteData = prefab.GetComponent<SpriteData>();
                        var rect = new Rectangle(
                            (entity.Position - spriteData.Origin).ToPoint(),
                            new Point(spriteData.SourceRect.Width, spriteData.SourceRect.Height));
                        if (spriteData != null && rect.Contains(position))
                        {
                            _window._entityWindow.Selection = entity.Id;
                        }
                    }
                }
            }
        }
    }
}
