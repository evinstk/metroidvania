using Nez;
using System;

namespace Game
{
    class AddedToEntity : Component
    {
        Action<Entity> _callback;

        public AddedToEntity(Action<Entity> callback)
        {
            _callback = callback;
        }

        public override void OnAddedToEntity()
        {
            _callback.Invoke(Entity);
            Entity.RemoveComponent(this);
        }
    }
}
