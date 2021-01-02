using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    [EditorWindow]
    class LayerWindow : Component
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
            if (roomData == null) return;

            if (ImGui.Begin("Layer"))
            {
                ImGui.InputText("##layerName", ref roomData.Layers[EditorState.SelectedLayerIndex].Name, 25);
                for (var i = roomData.Layers.Count - 1; i >= 0; --i)
                {
                    ImGui.RadioButton((i + 1).ToString() + ": " + roomData.Layers[i].Name, ref EditorState.SelectedLayerIndex, i);
                }
                if (NezImGui.CenteredButton("Add Layer", 1f))
                {
                    roomData.Layers.Add(new RoomLayer());
                }
                ImGui.End();
            }
        }
    }
}
