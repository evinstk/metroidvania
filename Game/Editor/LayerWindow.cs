using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using System.Collections.Generic;
using Num = System.Numerics;

namespace Game.Editor
{
    [EditorWindow]
    class LayerWindow : Component
    {
        struct LayerInspector
        {
            public string Name;
            public List<AbstractTypeInspector> Inspectors;
        }

        List<LayerInspector> _inspectors = new List<LayerInspector>();

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
            EditorState.OnRoomChange += HandleRoomChange;
            GenerateInspectors();
        }

        public override void OnRemovedFromEntity()
        {
            EditorState.OnRoomChange -= HandleRoomChange;
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void HandleRoomChange()
        {
            GenerateInspectors();
        }

        void GenerateInspectors()
        {
            _inspectors = new List<LayerInspector>();

            var roomData = EditorState.RoomData;
            if (roomData == null) return;

            for (var i = roomData.Layers.Count - 1; i >= 0; --i)
            {
                var layer = roomData.Layers[i];
                _inspectors.Add(new LayerInspector
                {
                    Name = layer.Name,
                    Inspectors = TypeInspectorUtils.GetInspectableProperties(layer),
                });
            }
        }

        List<RoomLayer> _tempLayers = new List<RoomLayer>();
        void Draw()
        {
            var roomData = EditorState.RoomData;
            if (roomData == null) return;

            if (ImGui.Begin("Layer"))
            {
                _tempLayers.Clear();
                _tempLayers.AddRange(roomData.Layers);
                _tempLayers.Sort((a, b) => a.RenderLayer.CompareTo(b.RenderLayer));
                for (var i = 0; i < _tempLayers.Count; ++i)
                {
                    var layer = _tempLayers[i];
                    var layerIndex = roomData.Layers.IndexOf(layer);
                    ImGui.RadioButton((i + 1).ToString() + ": " + _tempLayers[i].Name, ref EditorState.SelectedLayerIndex, layerIndex);
                }

                foreach (var layerInspector in _inspectors)
                {
                    ImGui.Indent();
                    NezImGui.BeginBorderedGroup();

                    if (ImGui.CollapsingHeader($"{layerInspector.Name}"))
                    {
                        foreach (var inspector in layerInspector.Inspectors)
                            inspector.Draw();
                    }

                    NezImGui.EndBorderedGroup(new Num.Vector2(4, 1), new Num.Vector2(4, 2));
                    ImGui.Unindent();
                }

                if (NezImGui.CenteredButton("Add Layer", 1f))
                {
                    roomData.Layers.Add(new RoomLayer());
                    GenerateInspectors();
                }

                ImGui.End();
            }
        }
    }
}
