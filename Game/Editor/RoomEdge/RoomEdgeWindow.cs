using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor.RoomEdge
{
    [EditorWindow]
    class RoomEdgeWindow : Component
    {
        RoomEdgeManager _roomEdgeManager = Core.GetGlobalManager<RoomEdgeManager>();
        RoomManager _roomManager = Core.GetGlobalManager<RoomManager>();

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
            if (EditorState.RoomData == null) return;

            if (ImGui.Begin("Room Edges", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();
                DrawMain();

                ImGui.End();
            }
        }

        void DrawMain()
        {
            var roomData = EditorState.RoomData;

            RoomEdge removedEdge = null;
            var edges = _roomEdgeManager.GetEdges(roomData.Id);
            for (var i = 0; i < edges.Count; ++i)
            {
                var edge = edges[i];

                ImGui.PushID(i * 3);
                var thisRoomIndex = edge.Rooms.FindIndex(r => r.RoomId == roomData.Id);
                ImGui.DragInt2(string.Empty, ref edge.Rooms[thisRoomIndex].Position.X);
                ImGui.PopID();

                ImGui.PushID(i * 3 + 1);
                var otherRoomIndex = 1 - thisRoomIndex;
                _roomManager.Combo(string.Empty, ref edge.Rooms[otherRoomIndex].RoomId);
                ImGui.DragInt2(string.Empty, ref edge.Rooms[otherRoomIndex].Position.X);
                ImGui.PopID();

                ImGui.PushID(i * 3 + 1);
                if (ImGui.Button("Save"))
                    _roomEdgeManager.Save(edge);
                ImGui.SameLine();
                if (ImGui.Button("Delete"))
                    removedEdge = edge;
                ImGui.PopID();

                if (i < edges.Count - 1)
                    ImGui.Separator();
            }

            if (NezImGui.CenteredButton("Add Room Edge", 1f))
            {
                var newEdge = _roomEdgeManager.NewResource();
                newEdge.Rooms[0].RoomId = roomData.Id;
                _roomEdgeManager.Save(newEdge);
            }

            if (removedEdge != null)
                _roomEdgeManager.Delete(removedEdge);
        }

        void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save All"))
                        _roomEdgeManager.SaveAll();
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }
    }
}
