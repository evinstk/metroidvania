using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    class EntityWindow : Component
    {
        public string Selection;

        RoomWindow _roomWindow;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
            _roomWindow = Entity.GetComponentStrict<RoomWindow>();
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            if (ImGui.Begin("Entity"))
            {
                var entity = _roomWindow.RoomData?.Entities.Find(e => e.Id == Selection);
                if (entity != null)
                {
                    if (NezImGui.CenteredButton("Remove Entity", 1f))
                    {
                        _roomWindow.RoomData.Entities.Remove(entity);
                    }
                }
                ImGui.End();
            }
        }
    }
}
