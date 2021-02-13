using Nez;

namespace Game
{
    class Sentry : Component, IUpdatable
    {
        enum States
        {
            Normal,
            Dead,
        }

        public int Health = 2;

        States _state = States.Normal;
        float _timer = 0;

        public void OnHurt(Hurtable hurtable)
        {
            if (Health > 0)
            {
                --Health;
                if (Health <= 0)
                    SetState(States.Dead);
            }
        }

        public void Update()
        {
            _timer += Time.DeltaTime;

            // NORMAL STATE
            if (_state == States.Normal)
            {
            }
            // DEAD STATE
            else if (_state == States.Dead)
            {
                if (Timer.OnTime(_timer, 2f))
                    Entity.Destroy();
            }
        }

        void SetState(States state)
        {
            _state = state;
            _timer = 0;
        }
    }
}
