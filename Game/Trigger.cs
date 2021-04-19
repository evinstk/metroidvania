using Nez;
using System;

namespace Game
{
    class Trigger : Component, IUpdatable
    {
        Func<Trigger, bool> _condition;
        Action<Trigger> _action;
        bool _noRepeat;

        bool _trueConditionLastFrame;

        public Trigger(Func<Trigger, bool> condition, Action<Trigger> action, bool noRepeat = false)
        {
            _condition = condition;
            _action = action;
            _noRepeat = noRepeat;
        }

        public void Update()
        {
            var result = _condition.Invoke(this);
            if (result)
            {
                if (!_noRepeat || !_trueConditionLastFrame)
                {
                    _action.Invoke(this);
                }
                _trueConditionLastFrame = true;
            }
            else
            {
                _trueConditionLastFrame = false;
            }
        }
    }
}
