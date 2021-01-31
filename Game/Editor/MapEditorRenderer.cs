using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game.Editor
{
    class MapEditorRenderer : RenderableComponent, IUpdatable
    {
        public override RectangleF Bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    var room = _room;
                    if (room == null)
                    {
                        _bounds = new RectangleF(0, 0, 1, 1);
                    }
                    else
                    {
                        var position = room.WorldPosition + _localOffset;
                        _bounds = new RectangleF
                        {
                            X = position.X,
                            Y = position.Y,
                            Width = room.RoomWidth * room.TileWidth,
                            Height = room.RoomHeight * room.TileHeight,
                        };
                    }
                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }

        public string RoomId;
        RoomData _room => RoomId != null ? _roomManager.GetResource(RoomId) : EditorState.RoomData;

        RoomManager _roomManager;

        public override void OnAddedToEntity()
        {
            _roomManager = Core.GetGlobalManager<RoomManager>();
        }

        List<RoomLayer> _tempList = new List<RoomLayer>();
        public override void Render(Batcher batcher, Camera camera)
        {
            var roomData = _room;
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
                        var texture = Core.Scene.Content.LoadTexture(ContentPath.Textures + tile.Tileset);
                        batcher.Draw(
                            texture,
                            new Rectangle(tile.LayerLocation * roomData.TileSize + LocalOffset.ToPoint(), roomData.TileSize),
                            // TODO: use tileset tile size
                            new Rectangle(tile.TilesetLocation * new Point(16, 16), new Point(16, 16)),
                            Color.White);
                    }
                }
            }
        }

        bool _nullLastFrame = false;
        Vector2 _lastSize = new Vector2(EditorState.RoomData?.RoomWidth ?? 0, EditorState.RoomData?.RoomHeight ?? 0);
        Vector2 _lastTileSize = new Vector2(EditorState.RoomData?.TileWidth ?? 0, EditorState.RoomData?.TileHeight ?? 0);
        public void Update()
        {
            var roomData = _room;
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
