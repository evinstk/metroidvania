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
                        if (tile.LayerLocation.X < roomData.Width && tile.LayerLocation.Y < roomData.Height)
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
        }

        bool _nullLastFrame = false;
        Vector2 _lastSize = new Vector2(EditorState.RoomData?.Width ?? 0, EditorState.RoomData?.Height ?? 0);
        Vector2 _lastTileSize = new Vector2(EditorState.RoomData?.TileWidth ?? 0, EditorState.RoomData?.TileHeight ?? 0);
        public void Update()
        {
            var nullThisFrame = EditorState.RoomData == null;
            var currSize = new Vector2(EditorState.RoomData?.Width ?? 0, EditorState.RoomData?.Height ?? 0);
            var currTileSize = new Vector2(EditorState.RoomData?.TileWidth ?? 0, EditorState.RoomData?.TileHeight ?? 0);
            if ((_nullLastFrame && !nullThisFrame) || _lastSize != currSize || _lastTileSize != currTileSize)
                _areBoundsDirty = true;

            _nullLastFrame = nullThisFrame;
            _lastSize = currSize;
            _lastTileSize = currTileSize;
        }
    }
}
