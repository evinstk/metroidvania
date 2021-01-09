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
                        if (entity.Select(position))
                            selection = entity.Id;
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
