using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor.Prefab
{
    class BoxColliderData : PrefabComponent
    {
        public Vector2 Size;

        public override void AddToEntity(Entity entity)
        {
            var collider = entity.AddComponent<BoxCollider>();
            collider.SetSize(Size.X, Size.Y);
        }
    }
}
