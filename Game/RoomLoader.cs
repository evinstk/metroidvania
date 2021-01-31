using Game.Editor;
using Nez;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Game.Scripting;
using Game.Editor.Prefab;
using Game.Editor.World;

namespace Game
{
    class RoomLoader : Component, IUpdatable
    {
        public int LoadRadius = 512;
        public RectangleF CurrentRoomBounds => _roomBounds.Find(b => b.WorldRoomId == _currWorldRoomId).Collider.Bounds;

        string _currWorldRoomId;
        string _checkpointId;

        WorldManager _worldManager = Core.GetGlobalManager<WorldManager>();
        RoomManager _roomManager = Core.GetGlobalManager<RoomManager>();

        struct RoomBounds
        {
            public Collider Collider;
            public string WorldRoomId;
        }
        List<RoomBounds> _roomBounds = new List<RoomBounds>();
        HashSet<string> _loadedWorldRoomIds = new HashSet<string>();

        MapScript _scripting;

        public RoomLoader(string worldRoomId, string checkpointId)
        {
            _currWorldRoomId = worldRoomId;
            Insist.IsNotNull(_currWorldRoomId);
            _checkpointId = checkpointId;
        }

        public override void OnAddedToEntity()
        {
            _scripting = Core.Scene.FindComponentOfType<MapScript>();
            Insist.IsNotNull(_scripting);

            var world = _worldManager.GetWorldRoom(_currWorldRoomId).World;
            foreach (var worldRoom in world.Rooms)
            {
                var room = _roomManager.GetResource(worldRoom.RoomId);
                var worldPosition = room.WorldPosition + worldRoom.Position.ToVector2();
                var collider = new BoxCollider(
                    new Rectangle(
                        worldPosition.ToPoint(),
                        new Point(
                            room.TileWidth * room.RoomWidth,
                            room.TileHeight * room.RoomHeight)));
                collider.Entity = Entity;
                Flags.SetFlagExclusive(ref collider.PhysicsLayer, PhysicsLayer.Room);
                Physics.AddCollider(collider);
                _roomBounds.Add(new RoomBounds
                {
                    Collider = collider,
                    WorldRoomId = worldRoom.Id,
                });
            }

            LoadRoom(_currWorldRoomId);

            var checkpoints = Entity.Scene.FindComponentsOfType<Checkpoint>();
            foreach (var checkpoint in checkpoints)
            {
                if (checkpoint.GetComponent<RoomEntityComponent>().RoomEntityId == _checkpointId || _checkpointId == null)
                {
                    Core.GetGlobalManager<PrefabManager>()
                        .GetResource("WJUFDAADKEXSLBFZLTMHNQVZGDJPHFUVUVMHXC")
                        .CreateEntity("hero", Entity.Scene)
                        .SetPosition(checkpoint.Entity.Position);
                    break;
                }
            }
        }

        void LoadRoom(string worldRoomId)
        {
            if (_loadedWorldRoomIds.Contains(worldRoomId)) return;

            var worldRoom = _worldManager.GetWorldRoom(worldRoomId);
            var roomData = _roomManager.GetResource(worldRoom.RoomId);
            var worldPosition = roomData.WorldPosition + worldRoom.Position.ToVector2();

            var mapEntity = Core.Scene.CreateEntity($"Map: {roomData.DisplayName}").SetPosition(worldPosition);
            var count = roomData.Layers.Count;
            for (var i = 0; i < count; ++i)
            {
                var layer = roomData.Layers[i];
                var layerEntity = Core.Scene.CreateEntity(layer.Name);
                layerEntity.Transform.SetParent(mapEntity.Transform);
                layerEntity.AddComponent(new MapRenderer(roomData, i)).SetRenderLayer(layer.RenderLayer);

                if (layer.PhysicsLayer.Mask != 0)
                {
                    var mapCollider = layerEntity.AddComponent(new MapCollider(roomData, i));
                    mapCollider.PhysicsLayer = layer.PhysicsLayer.Mask;
                    if (layer.IsOverlay)
                    {
                        mapCollider.IsTrigger = true;
                        layerEntity.AddComponent<Overlay>();
                    }
                }
                Debug.LogIf(
                    layer.PhysicsLayer.Mask == 0 && layer.IsOverlay,
                    $"Layer {layer.Name} has no physics mask but is overlay. No effect.");
            }

            if (roomData.Script != null)
                _scripting.Queue(roomData.Id, roomData.Script, roomData.RoomVariables.Variables);

            foreach (var entityData in roomData.Entities)
                entityData.CreateEntity(Entity.Scene, worldRoomId, worldRoom.Position.ToVector2());

            _loadedWorldRoomIds.Add(worldRoomId);
        }

        Collider[] _results = new Collider[8];
        public void Update()
        {
            var playerEntity = Core.Scene.FindComponentOfType<PlayerController>()?.Entity;
            var mask = 0;
            Flags.SetFlag(ref mask, PhysicsLayer.Room);
            if (playerEntity != null && !playerEntity.IsDestroyed)
            {
                var playerPosition = playerEntity.Position;
                var count = Physics.OverlapCircleAll(playerPosition, LoadRadius, _results, mask);
                for (var i = 0; i < count; ++i)
                {
                    var collider = _results[i];
                    var roomBounds = _roomBounds.Find(b => b.Collider == collider);
                    if (roomBounds.WorldRoomId != null)
                    {
                        LoadRoom(roomBounds.WorldRoomId);
                        if (roomBounds.Collider.Bounds.Contains(playerPosition))
                        {
                            _currWorldRoomId = roomBounds.WorldRoomId;
                        }
                    }
                }
            }
        }
    }
}
