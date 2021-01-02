using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor
{
    class MapEditorRenderer : RenderableComponent, IUpdatable
    {
        public override float Width =>
            EditorState.RoomData != null ? EditorState.RoomData.Width * EditorState.RoomData.TileWidth : 1;
        public override float Height =>
            EditorState.RoomData != null ? EditorState.RoomData.Height * EditorState.RoomData.TileHeight : 1;

        public override void Render(Batcher batcher, Camera camera)
        {
            var roomData = EditorState.RoomData;
            if (roomData != null)
            {
                foreach (var layer in roomData.Layers)
                {
                    foreach (var tile in layer.Tiles)
                    {
                        var texture = Core.Scene.Content.LoadTexture("Content/Textures/" + tile.Tileset);
                        batcher.Draw(
                            texture,
                            new Rectangle(tile.LayerLocation * roomData.TileSize, roomData.TileSize),
                            // TODO: use tileset tile size
                            new Rectangle(tile.TilesetLocation * new Point(16, 16), new Point(16, 16)),
                            Color.White);
                    }
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
