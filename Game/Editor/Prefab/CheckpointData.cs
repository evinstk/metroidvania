using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor.Prefab
{
    class CheckpointData : DataComponent
    {
        public Vector2 Size = new Vector2(32, 32);
        public PhysicsLayerData PhysicsLayer = new PhysicsLayerData();

        public override void AddToEntity(Entity entity)
        {
            var checkpoint = entity.AddComponent<Checkpoint>();
            checkpoint.Size = Size;
            checkpoint.PhysicsMask = PhysicsLayer.Mask;
        }
    }
}
