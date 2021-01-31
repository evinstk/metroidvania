using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor
{
    class PrefabEditorRenderer : RenderableComponent, IUpdatable
    {
        public override float Width =>
            EditorState.RoomData != null ? EditorState.RoomData.RoomWidth * EditorState.RoomData.TileWidth : 1;
        public override float Height =>
            EditorState.RoomData != null ? EditorState.RoomData.RoomHeight * EditorState.RoomData.TileHeight : 1;

        PrefabManager _prefabManager;

        public override void OnAddedToEntity()
        {
            _prefabManager = Core.GetGlobalManager<PrefabManager>();
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var roomData = EditorState.RoomData;
            if (roomData != null)
            {
                foreach (var entity in roomData.Entities)
                    entity.Render(batcher);
            }
        }

        bool _nullLastFrame = false;
        Vector2 _lastSize = new Vector2(EditorState.RoomData?.RoomWidth ?? 0, EditorState.RoomData?.RoomHeight ?? 0);
        Vector2 _lastTileSize = new Vector2(EditorState.RoomData?.TileWidth ?? 0, EditorState.RoomData?.TileHeight ?? 0);
        public void Update()
        {
            var roomData = EditorState.RoomData;
            // TODO: WorldPosition might be expensive to call every frame (also in PrefabEditorRenderer)
            Entity.Position = roomData?.WorldPosition ?? Vector2.Zero;
            var nullThisFrame = roomData == null;
            var currSize = new Vector2(roomData?.RoomWidth ?? 0, roomData?.RoomHeight ?? 0);
            var currTileSize = new Vector2(roomData?.TileWidth ?? 0, roomData?.TileHeight ?? 0);
            if ((_nullLastFrame && !nullThisFrame) || _lastSize != currSize || _lastTileSize != currTileSize)
                _areBoundsDirty = true;

            _nullLastFrame = nullThisFrame;
            _lastSize = currSize;
            _lastTileSize = currTileSize;
        }
    }
}
