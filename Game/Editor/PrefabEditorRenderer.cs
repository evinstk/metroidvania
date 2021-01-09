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

                    var rectangleData = prefab?.Components.Find(c => c is RectangleRendererData) as RectangleRendererData;
                    if (rectangleData != null)
                    {
                        var rect = new RectangleF(
                            entity.Position - rectangleData.Size / 2,
                            rectangleData.Size);
                        batcher.DrawRect(
                            rect,
                            rectangleData.Color);
                    }

                    var spriteData = prefab?.Components.Find(c => c is SpriteData) as SpriteData;
                    var texture = spriteData?.TextureData.Texture;
                    if (spriteData != null && texture != null)
                    {
                        batcher.Draw(
                            texture,
                            entity.Position - spriteData.Origin,
                            spriteData.SourceRect,
                            spriteData.Color);
                    }

                    foreach (var component in entity.Components)
                    {
                        if (component is AreaData areaData)
                        {
                            var color = Microsoft.Xna.Framework.Color.DarkMagenta;
                            color.A = 1;
                            batcher.DrawRect(
                                entity.Position,
                                areaData.Size.X,
                                areaData.Size.Y,
                                color);
                        }
                    }
                }
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
