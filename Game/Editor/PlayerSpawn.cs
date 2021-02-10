using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor
{
    class PlayerSpawnData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<PlayerSpawn>();
        }

        public override void Render(Batcher batcher, Vector2 position)
        {
            batcher.DrawPixel(position, Color.Indigo, 4);
        }
    }

    class PlayerSpawn : Component
    {
    }
}
