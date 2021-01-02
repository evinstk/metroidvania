using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    [EditorWindow]
    class LayerWindow : Component
    {
        public RoomLayer SelectedLayer =>
            _selectedLayer >= 0 && _selectedLayer < EditorState.RoomData?.Layers.Count
            ? EditorState.RoomData.Layers[_selectedLayer]
            : null;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        int _selectedLayer = 0;
        void Draw()
        {
            var roomData = EditorState.RoomData;
            if (roomData == null) return;

            if (ImGui.Begin("Layer"))
            {
                ImGui.InputText("##layerName", ref roomData.Layers[_selectedLayer].Name, 25);
                for (var i = roomData.Layers.Count - 1; i >= 0; --i)
                {
                    ImGui.RadioButton((i + 1).ToString() + ": " + roomData.Layers[i].Name, ref _selectedLayer, i);
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
