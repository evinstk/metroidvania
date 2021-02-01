using ImGuiNET;

namespace Game.Editor.World
{
    class WorldManager : Manager<World>
    {
        public override string Path => ContentPath.Worlds;

        public bool WorldRoomCombo(string roomId, ref string worldRoomId)
        {
            var ret = false;

            var label = string.Empty;
            foreach (var world in _resources)
            {
                foreach (var worldRoom in world.Data.Rooms)
                {
                    if (worldRoom.Id == worldRoomId)
                        label = $"{world.Data.DisplayName}: {worldRoom.Name}";
                }
            }

            if (ImGui.BeginCombo("World Room", label))
            {
                foreach (var world in _resources)
                {
                    foreach (var worldRoom in world.Data.Rooms)
                    {
                        ImGui.PushID(worldRoom.Id);
                        if (worldRoom.RoomId == roomId && ImGui.Selectable($"{world.Data.DisplayName}: {worldRoom.Name}"))
                        {
                            worldRoomId = worldRoom.Id;
                            ret = true;
                        }
                        ImGui.PopID();
                    }
                }
                ImGui.EndCombo();
            }
            return ret;
        }

        public WorldRoom GetWorldRoom(string worldRoomId)
        {
            foreach (var world in _resources)
            {
                foreach (var worldRoom in world.Data.Rooms)
                {
                    if (worldRoom.Id == worldRoomId)
                        return worldRoom;
                }
            }
            return null;
        }
    }

}
