using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Nez;

namespace Game.Hit
{
    class IntValueHitListenerData : DataComponent
    {
        public IntReference CurrHealth = new IntReference();

        public override void AddToEntity(Entity entity)
        {
            var listener = entity.AddComponent<IntValueHitListener>();
            listener.CurrHealth = CurrHealth.Dereference();
        }
    }

    class IntValueHitListener : Component, IHitListener
    {
        public IntValue CurrHealth;

        public void OnHit()
        {
            --CurrHealth.Value;
            if (CurrHealth.Value == 0)
                Entity.Destroy();
        }
    }
}
