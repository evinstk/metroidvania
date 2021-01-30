﻿using Game.Editor;
using Nez;
using Microsoft.Xna.Framework;
using Game.Editor.RoomEdge;
using System.Collections.Generic;
using Game.Scripting;
using System;
using Game.Editor.Prefab;

namespace Game
{
    class RoomLoader : Component, IUpdatable
    {
        public RectangleF CurrentRoomBounds => _loadedRooms[_currRoomId];

        string _currRoomId;
        string _checkpointId;

        RoomManager _roomManager = Core.GetGlobalManager<RoomManager>();
        RoomEdgeManager _roomEdgeManager = Core.GetGlobalManager<RoomEdgeManager>();
        Dictionary<string, RectangleF> _loadedRooms = new Dictionary<string, RectangleF>();

        MapScript _scripting;

        public RoomLoader(string roomDataId, string checkpointId)
        {
            _currRoomId = roomDataId;
            Insist.IsNotNull(_currRoomId);
            _checkpointId = checkpointId;
        }

        public override void OnAddedToEntity()
        {
            _scripting = Core.Scene.FindComponentOfType<MapScript>();
            Insist.IsNotNull(_scripting);
            LoadRoomAndAdjacent(_currRoomId, Vector2.Zero);

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

        public void Update()
        {
            var playerEntity = Core.Scene.FindComponentOfType<PlayerController>()?.Entity;
            if (playerEntity != null && !playerEntity.IsDestroyed)
            {
                var position = playerEntity.Position;
                var currBounds = _loadedRooms[_currRoomId];
                if (!currBounds.Contains(position))
                {
                    var lastRoomId = _currRoomId;
                    foreach (var room in _loadedRooms)
                    {
                        if (room.Value.Contains(position))
                        {
                            _currRoomId = room.Key;
                            LoadRoomAndAdjacent(_currRoomId, room.Value.Location);
                            GC.Collect();
                            break;
                        }
                    }
                    // new room must have been found
                    Debug.LogIf(lastRoomId == _currRoomId, "Out of bounds.");
                }
            }
        }

        void LoadRoomAndAdjacent(string roomDataId, Vector2 offset)
        {
            SetupRoom(roomDataId, offset);
            var edges = _roomEdgeManager.GetEdges(roomDataId);
            foreach (var edge in edges)
            {
                var adjacentRoomIndex = edge.Rooms.FindIndex(r => r.RoomId != roomDataId);
                var currRoomIndex = 1 - adjacentRoomIndex;
                SetupRoom(
                    edge.Rooms[adjacentRoomIndex].RoomId,
                    (edge.Rooms[currRoomIndex].Position - edge.Rooms[adjacentRoomIndex].Position).ToVector2() + offset);
            }
        }

        void SetupRoom(string roomDataId, Vector2 offset)
        {
            if (_loadedRooms.ContainsKey(roomDataId)) return;

            var roomData = _roomManager.GetResource(roomDataId);
            var worldPosition = roomData.WorldPosition + offset;

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
                entityData.CreateEntity(Entity.Scene, offset);

            _loadedRooms.Add(
                roomData.Id,
                new RectangleF(
                    worldPosition,
                    new Vector2(
                        roomData.TileWidth * roomData.RoomWidth,
                        roomData.TileHeight * roomData.RoomHeight)));
        }
    }
}
