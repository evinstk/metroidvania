using Game.Editor.Prefab;
using Nez;

namespace Game.Editor
{
    class PrefabEditorRenderer : RenderableComponent, IUpdatable
    {
        public override float Width =>
            EditorState.RoomData != null ? EditorState.RoomData.Width * EditorState.RoomData.TileWidth : 1;
        public override float Height =>
            EditorState.RoomData != null ? EditorState.RoomData.Height * EditorState.RoomData.TileHeight : 1;

        PrefabManager _prefabManager;

        public override void OnAddedToEntity()
        {
            var windows = Core.Scene.FindEntity("windows");
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
        Microsoft.Xna.Framework.Vector2 _lastSize = new Microsoft.Xna.Framework.Vector2(EditorState.RoomData?.Width ?? 0, EditorState.RoomData?.Height ?? 0);
        Microsoft.Xna.Framework.Vector2 _lastTileSize = new Microsoft.Xna.Framework.Vector2(EditorState.RoomData?.TileWidth ?? 0, EditorState.RoomData?.TileHeight ?? 0);
        public void Update()
        {
            var nullThisFrame = EditorState.RoomData == null;
            var currSize = new Microsoft.Xna.Framework.Vector2(EditorState.RoomData?.Width ?? 0, EditorState.RoomData?.Height ?? 0);
            var currTileSize = new Microsoft.Xna.Framework.Vector2(EditorState.RoomData?.TileWidth ?? 0, EditorState.RoomData?.TileHeight ?? 0);
            if ((_nullLastFrame && !nullThisFrame) || _lastSize != currSize || _lastTileSize != currTileSize)
                _areBoundsDirty = true;

            _nullLastFrame = nullThisFrame;
            _lastSize = currSize;
            _lastTileSize = currTileSize;
        }
    }
}
