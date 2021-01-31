using Nez;
using System.Collections.Generic;

namespace Game.Editor.World
{
    class WorldEditorRenderer : Component, IUpdatable
    {
        Dictionary<string, MapEditorRenderer> _renderers = new Dictionary<string, MapEditorRenderer>();
        List<string> _toRemove = new List<string>();

        public void Update()
        {
            var world = WorldEditorState.World;
            if (world == null) return;

            foreach (var worldRoom in world.Rooms)
            {
                if (!_renderers.TryGetValue(worldRoom.Id, out var renderer))
                {
                    renderer = Entity.AddComponent<MapEditorRenderer>();
                    renderer.RoomId = worldRoom.RoomId;
                    _renderers.Add(worldRoom.Id, renderer);
                }
                renderer.LocalOffset = worldRoom.Position.ToVector2();
            }

            foreach (var id in _renderers.Keys)
            {
                if (world.Rooms.Find(r => r.Id == id) == null)
                    _toRemove.Add(id);
            }
            foreach (var id in _toRemove)
            {
                var removedRenderer = _renderers[id];
                Entity.RemoveComponent(removedRenderer);
                _renderers.Remove(id);
            }
            _toRemove.Clear();
        }
    }
}
