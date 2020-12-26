using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor
{
    class MapRenderer : RenderableComponent
    {
        public override float Width => 1;
        public override float Height => 1;

        RoomWindow _roomWindow;

        public override void OnAddedToEntity()
        {
            _roomWindow = Core.Scene.FindEntity("windows").GetComponent<RoomWindow>();
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var roomData = _roomWindow.RoomData;
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
    }
}
