using Game.Editor.Prefab;
using Nez;
using System;

namespace Game.Editor
{
    class RoomManager : Manager<RoomData>
    {
        public override string Path => ContentPath.Rooms;

        public override void Initialize()
        {
            foreach (var room in _resources)
                room.Data.Initialize();
            Core.GetGlobalManager<PrefabManager>().OnPrefabChange += SyncEntityOnlyComponents;
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

        public void SyncEntityOnlyComponents()
        {
            foreach (var room in _resources)
            {
                foreach (var entity in room.Data.Entities)
                {
                    entity.SyncEntityOnlyComponents();
                }
            }
            OnEntityOnlySync?.Invoke();
        }

        public event Action OnEntityOnlySync;
    }
}
