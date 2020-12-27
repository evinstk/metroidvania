using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
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
            if (ImGui.Begin("Transport"))
            {
                ImGui.Button("Play");
                ImGui.SameLine();
                ImGui.Button("Stop");
                ImGui.End();
            }
        }
    }
}
