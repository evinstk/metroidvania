using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor.Prefab
{
    class BoxColliderData : DataComponent
    {
        public Vector2 Size;
        public PhysicsLayerData PhysicsLayer = new PhysicsLayerData();
        public PhysicsLayerData CollidesWithLayers = new PhysicsLayerData();

        public override void AddToEntity(Entity entity)
        {
            var collider = entity.AddComponent<BoxCollider>();
            collider.SetSize(Size.X, Size.Y);
            collider.PhysicsLayer = PhysicsLayer.Mask;
            collider.CollidesWithLayers = CollidesWithLayers.Mask;
        }
    }
}
