using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor.Prefab
{
    class BoxColliderData : DataComponent
    {
        public Vector2 Size;
        public Vector2 Offset;
        public PhysicsLayerData PhysicsLayer = new PhysicsLayerData();
        public PhysicsLayerData CollidesWithLayers = new PhysicsLayerData();
        public bool IsTrigger;

        public override void AddToEntity(Entity entity)
        {
            var collider = entity.AddComponent<BoxCollider>();
            collider.SetSize(Size.X, Size.Y);
            collider.LocalOffset = Offset;
            collider.PhysicsLayer = PhysicsLayer.Mask;
            collider.CollidesWithLayers = CollidesWithLayers.Mask;
            collider.IsTrigger = IsTrigger;
        }
    }
}
