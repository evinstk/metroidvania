using Nez;
using System;

namespace Game
{
    class Trigger : Component, IUpdatable
    {
        public Func<Trigger, bool> Condition;
        public Action<Trigger> Action;

        public void Update()
        {
            var result = Condition.Invoke(this);
            if (result)
                Action.Invoke(this);
        }
    }
}
