using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game
{
    class Transport : Component
    {
        StartRoom _startRoom;

        public Transport(StartRoom startRoom)
        {
            _startRoom = startRoom;
        }

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
            if (ImGui.Begin("Transport"))
            {
                if (ImGui.Button("Stop"))
                {
                    Core.Scene = new EditorScene(new EditorInit
                    {
                        World = _startRoom.World,
                        RoomId = _startRoom.RoomId,
                    });
                }
                ImGui.End();
            }
        }
    }
}
