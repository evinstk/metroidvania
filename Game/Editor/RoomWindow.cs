using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using System.Collections.Generic;
using Num = System.Numerics;

namespace Game.Editor
{
    [EditorWindow]
    class RoomWindow : Component
    {
        List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

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
            ImGui.SetNextWindowPos(new Num.Vector2(25, 500), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Num.Vector2(300, 300), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Room", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();
                DrawRoomData();

                ImGui.End();
            }
        }

        //string _newRoomName;
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
                Core.GetGlobalManager<RoomManager>().SaveAll();
        }

        void DrawRoomData()
        {
            var roomManager = Core.GetGlobalManager<RoomManager>();
            if (roomManager.Combo("Room", EditorState.SelectedRoomId, ref EditorState.SelectedRoomId))
            {
                var roomData = roomManager.GetResource(EditorState.SelectedRoomId);
                _inspectors = TypeInspectorUtils.GetInspectableProperties(roomData);
            }

            foreach (var inspector in _inspectors)
            {
                inspector.Draw();
            }
        }
    }
}
