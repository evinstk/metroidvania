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
                {
                    var prefab = _prefabManager.GetResource(entity.PrefabId);
                    if (prefab == null)
                        continue;

                    var spriteData = prefab.Components.Find(c => c is SpriteData) as SpriteData;
                    if (spriteData == null)
                        continue;

                    var texture = spriteData.TextureData.Texture;
                    if (texture == null)
                        continue;

                    batcher.Draw(
                        texture,
                        entity.Position - spriteData.Origin,
                        spriteData.SourceRect,
                        spriteData.Color);
                }
            }
        }

        bool _nullLastFrame = false;
        public void Update()
        {
            var nullThisFrame = EditorState.RoomData == null;
            if (_nullLastFrame && !nullThisFrame)
                _areBoundsDirty = true;
            _nullLastFrame = nullThisFrame;
        }
    }
}
