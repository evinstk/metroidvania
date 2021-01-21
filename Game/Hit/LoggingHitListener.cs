using Game.Editor.Prefab;
using Nez;

namespace Game.Hit
{
    class LoggingHitListenerData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<LoggingHitListener>();
        }
    }

    class LoggingHitListener : Component, IHitListener
    {
        public void OnHit()
        {
            Debug.Log($"{Entity.Name} hit!");
        }
    }
}
