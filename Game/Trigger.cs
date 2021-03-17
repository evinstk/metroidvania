using Nez;
using System;

namespace Game
{
    class Trigger : Component, IUpdatable
    {
        Func<Trigger, bool> _condition;
        Action<Trigger> _action;

        public Trigger(Func<Trigger, bool> condition, Action<Trigger> action)
        {
            _condition = condition;
            _action = action;
        }

        public void Update()
        {
            var result = _condition.Invoke(this);
            if (result)
                _action.Invoke(this);
        }
    }
}
