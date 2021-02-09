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

    class BasicHitListener : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            Entity.Destroy();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
