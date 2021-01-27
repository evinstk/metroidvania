using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game.Editor
{
    class MapEditorRenderer : RenderableComponent, IUpdatable
    {
        public override float Width =>
            EditorState.RoomData != null ? EditorState.RoomData.Width * EditorState.RoomData.TileWidth : 1;
        public override float Height =>
            EditorState.RoomData != null ? EditorState.RoomData.Height * EditorState.RoomData.TileHeight : 1;

        List<RoomLayer> _tempList = new List<RoomLayer>();
        public override void Render(Batcher batcher, Camera camera)
        {
            var roomData = EditorState.RoomData;
            if (roomData != null)
            {
                _tempList.Clear();
                _tempList.AddRange(roomData.Layers);
                _tempList.Sort((a, b) => b.RenderLayer.CompareTo(a.RenderLayer));
                foreach (var layer in _tempList)
                {
                    if (layer.IsHidden) continue;
                    foreach (var tile in layer.Tiles)
                    {
                        if (tile.LayerLocation.X < roomData.Width && tile.LayerLocation.Y < roomData.Height)
                        {
                            var texture = Core.Scene.Content.LoadTexture(ContentPath.Textures + tile.Tileset);
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
