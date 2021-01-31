using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor.World
{
    [WorldEditorWindow]
    class WorldRoomWindow : Component
    {
        RoomManager _roomManager;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
            _roomManager = Core.GetGlobalManager<RoomManager>();
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            var world = WorldEditorState.World;
            if (world == null) return;

            if (ImGui.Begin("World Rooms"))
            {
                WorldRoom toRemove = null;
                foreach (var worldRoom in world.Rooms)
                {
                    ImGui.PushID(worldRoom.Id);
                    var room = _roomManager.GetResource(worldRoom.RoomId);
                    if (room == null) continue;
                    ImGui.InputText("Name", ref worldRoom.Name, 25);
                    ImGui.DragInt2("Position", ref worldRoom.Position.X);

                    ImGui.SameLine();

                    if (ImGui.Button("Remove"))
                        toRemove = worldRoom;
                    ImGui.PopID();
                }
                if (toRemove != null)
                    world.Rooms.Remove(toRemove);

                if (ImGui.Button("Add Room"))
                {
                    ImGui.OpenPopup("add-world-room");
                }

                var roomId = string.Empty;
                if (_roomManager.Popup("add-world-room", ref roomId))
                {
                    var room = _roomManager.GetResource(roomId);
                    world.Rooms.Add(new WorldRoom
                    {
                        RoomId = roomId,
                        Position = Point.Zero,
                        Name = room.Name,
                        World = world,
                    });
                }

                ImGui.End();
            }
        }
    }
}
