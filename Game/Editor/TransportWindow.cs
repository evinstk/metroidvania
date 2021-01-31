using Game.Editor.World;
using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    [EditorWindow]
    class TransportWindow : Component
    {
        WorldManager _worldManager;

        public override void OnAddedToEntity()
        {
            _worldManager = Core.GetGlobalManager<WorldManager>();
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            var roomData = EditorState.RoomData;
            if (roomData == null) return;

            if (ImGui.Begin("Transport"))
            {
                _worldManager.WorldRoomCombo(roomData.Id, ref EditorState.SelectedWorldId);

                if (EditorState.SelectedWorldId != null && ImGui.Button("Launch Room"))
                {
                    Core.Scene = new RoomScene(EditorState.SelectedWorldId);
                }
                ImGui.End();
            }
        }
    }
}
