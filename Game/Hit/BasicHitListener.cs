using Game.Editor.Prefab;
using Nez;

namespace Game.Hit
{
    class BasicHitListenerData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<BasicHitListener>();
        }
    }

    class BasicHitListener : Component, IHitListener
    {
        public void OnHit()
        {
            Entity.Destroy();
        }
    }
}
