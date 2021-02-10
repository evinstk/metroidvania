using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor
{
    class PlayerSpawnData : DataComponent
    {
        const int _pixelSize = 4;

        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<PlayerSpawn>();
        }

        public override void Render(Batcher batcher, Vector2 position)
        {
            batcher.DrawPixel(position, Color.Indigo, _pixelSize);
        }

        public override bool Select(Vector2 entityPosition, Vector2 mousePosition)
        {
            var pixelSize = new Vector2(_pixelSize);
            var rect = new RectangleF(entityPosition - pixelSize / 2, pixelSize);
            return rect.Contains(mousePosition);
        }
    }

    class PlayerSpawn : Component
    {
    }
}
