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
                    string selection = null;
                    foreach (var entity in roomData.Entities)
                    {
                        var prefab = entity.Prefab;
                        SpriteData spriteData = null;
                        RectangleRendererData rectRenderer = null;
                        if (prefab?.TryGetComponent(out spriteData) == true)
                        {
                            var rect = new Rectangle(
                                (entity.Position - spriteData.Origin).ToPoint(),
                                new Point(spriteData.SourceRect.Width, spriteData.SourceRect.Height));
                            if (rect.Contains(position))
                            {
                                selection = entity.Id;
                            }
                        }
                        else if (prefab?.TryGetComponent(out rectRenderer) == true)
                        {
                            var rect = new RectangleF(
                                entity.Position - rectRenderer.Size / 2,
                                rectRenderer.Size);
                            if (rect.Contains(position))
                            {
                                selection = entity.Id;
                            }
                        }
                        // fallback to area selections only if sprite entity not found
                        else if (selection == null)
                        {
                            var area = entity.Components.Find(c => c is AreaData) as AreaData;
                            if (area != null)
                            {
                                var bounds = new RectangleF(entity.Position, area.Size.ToVector2());
                                if (area != null && bounds.Contains(position))
                                {
                                    selection = entity.Id;
                                }
                            }
                        }
                    }
                    if (selection != null)
                    {
                        EditorState.SelectedEntityId = selection;
                    }
                }
            }
        }
    }
}
