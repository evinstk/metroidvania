﻿using Game.Editor;
using Nez;
using Microsoft.Xna.Framework;
using Game.Editor.RoomEdge;
using System.Collections.Generic;
using Game.Scripting;
using System;

namespace Game
{
    class RoomLoader : Component, IUpdatable
    {
        public RectangleF CurrentRoomBounds => _loadedRooms[_currRoomId];

        string _currRoomId;
        RoomManager _roomManager = Core.GetGlobalManager<RoomManager>();
        RoomEdgeManager _roomEdgeManager = Core.GetGlobalManager<RoomEdgeManager>();
        Dictionary<string, RectangleF> _loadedRooms = new Dictionary<string, RectangleF>();

        public RoomLoader(string roomDataId)
        {
            _currRoomId = roomDataId;
        }

        public override void Initialize()
        {
            Insist.IsNotNull(_currRoomId);
            LoadRoomAndAdjacent(_currRoomId, Vector2.Zero);
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

            var mapEntity = Core.Scene.CreateEntity($"Map: {roomData.DisplayName}").SetPosition(offset);
            var count = roomData.Layers.Count;
            for (var i = 0; i < count; ++i)
            {
                var layer = roomData.Layers[i];
                var layerEntity = Core.Scene.CreateEntity(layer.Name);
                layerEntity.Transform.SetParent(mapEntity.Transform);
                layerEntity.AddComponent(new MapRenderer(roomData, i)).SetRenderLayer(count - 1 - i);

                if (layer.HasColliders)
                {
                    var mapCollider = layerEntity.AddComponent(new MapCollider(roomData, i));
                    Flags.SetFlagExclusive(ref mapCollider.PhysicsLayer, PhysicsLayer.Terrain);
                }
            }

            foreach (var entityData in roomData.Entities)
                entityData.CreateEntity(Entity.Scene, offset);

            Core.Scene.CreateEntity($"Scripting: {roomData.DisplayName}").AddComponent(new MapScript(roomData.Script));

            _loadedRooms.Add(
                roomData.Id,
                new RectangleF(
                    offset,
                    new Vector2(
                        roomData.TileWidth * roomData.Width,
                        roomData.TileHeight * roomData.Height)));
        }
    }
}
