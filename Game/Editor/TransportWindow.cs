using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    [EditorWindow]
    class TransportWindow : Component
    {

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            var roomData = EditorState.RoomData;
            if (ImGui.Begin("Transport"))
            {
                if (ImGui.Button("Launch Room") && roomData != null)
                {
                    Core.Scene = new RoomScene(roomData.Id);
                }
                ImGui.End();
            }
        }
    }
}
