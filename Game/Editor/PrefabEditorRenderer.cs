using Game.Editor.Prefab;
using Nez;

namespace Game.Editor
{
    class PrefabEditorRenderer : RenderableComponent, IUpdatable
    {
        public override float Width =>
            _roomWindow.RoomData != null ? _roomWindow.RoomData.Width * _roomWindow.RoomData.TileWidth : 1;
        public override float Height =>
            _roomWindow.RoomData != null ? _roomWindow.RoomData.Height * _roomWindow.RoomData.TileHeight : 1;

        RoomWindow _roomWindow;
        PrefabManager _prefabManager;

        public override void OnAddedToEntity()
        {
            var windows = Core.Scene.FindEntity("windows");
            _roomWindow = windows.GetComponentStrict<RoomWindow>();
            _prefabManager = Core.GetGlobalManager<PrefabManager>();
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var roomData = _roomWindow.RoomData;
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
            var nullThisFrame = _roomWindow.RoomData == null;
            if (_nullLastFrame && !nullThisFrame)
                _areBoundsDirty = true;
            _nullLastFrame = nullThisFrame;
        }
    }
}
