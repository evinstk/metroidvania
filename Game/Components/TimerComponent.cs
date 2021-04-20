using Nez;
using System;

namespace Game.Components
{
    class TimerComponent : Component, IUpdatable
    {
        float _duration;
        Action<TimerComponent> _callback;

        public TimerComponent(float duration, Action<TimerComponent> callback)
        {
            _duration = duration;
            _callback = callback;
        }

        void Start(float duration)
        {
            _duration = duration;
        }

        public void Update()
        {
            if (_duration > 0)
            {
                _duration -= Time.DeltaTime;
                if (_duration <= 0 && _callback != null)
                {
                    _callback(this);
                }
            }
        }
    }
}
