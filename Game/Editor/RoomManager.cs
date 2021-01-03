using Nez;

namespace Game.Editor
{
    class RoomManager : Manager<RoomData>
    {
        public override string Path => ContentPath.Rooms;

        public RoomManager()
        {
            foreach (var room in _resources)
            {
                foreach (var entity in room.Data.Entities)
                {
                    entity.Room = room.Data;
                }
            }
        }

        public RoomEntity GetEntity(string Id)
        {
            foreach (var room in _resources)
            {
                foreach (var entity in room.Data.Entities)
                {
                    if (entity.Id == Id)
                        return entity;
                }
            }
            Debug.Log($"Entity {Id} not found");
            return null;
        }
    }
}
