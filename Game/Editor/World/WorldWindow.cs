using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using System.Collections.Generic;

namespace Game.Editor.World
{
    [WorldEditorWindow]
    class WorldWindow : Component
    {
        WorldManager _worldManager;
        List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

        public override void OnAddedToEntity()
        {
            _worldManager = Core.GetGlobalManager<WorldManager>();

            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
            WorldEditorState.OnSetWorld += GenerateInspectors;
        }

        public override void OnRemovedFromEntity()
        {
            WorldEditorState.OnSetWorld -= GenerateInspectors;
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            if (ImGui.Begin("World", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();

                var worldId = WorldEditorState.WorldId;
                if (_worldManager.Combo("World", ref worldId))
                    WorldEditorState.WorldId = worldId;

                if (NezImGui.CenteredButton("New World", 1f))
                {
                    var resource = _worldManager.NewResource();
                    WorldEditorState.WorldId = resource.Id;
                }

                foreach (var inspector in _inspectors)
                    inspector.Draw();

                if (worldId != null && ImGui.Button("Delete"))
                    ImGui.OpenPopup("delete-world");

                if (ImGui.BeginPopupModal("delete-world"))
                {
                    var world = WorldEditorState.World;
                    ImGui.Text($"Delete world {world.DisplayName}?");
                    if (ImGui.Button("Yes"))
                    {
                        WorldEditorState.WorldId = null;
                        _worldManager.Delete(worldId);
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                        ImGui.CloseCurrentPopup();
                    ImGui.EndPopup();
                }

                ImGui.End();
            }
        }

        void DrawMenuBar()
        {
            var saveAll = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save All"))
                        saveAll = true;
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (saveAll)
                _worldManager.SaveAll();
        }

        void GenerateInspectors()
        {
            var world = WorldEditorState.World;
            if (world == null)
            {
                _inspectors = new List<AbstractTypeInspector>();
                return;
            }

            _inspectors = TypeInspectorUtils.GetInspectableProperties(world);
        }
    }
}
