using Nez;
using Nez.Sprites;

namespace Game
{
    class Sentry : Component, IUpdatable
    {
        enum States
        {
            Normal,
            Dead,
        }

        public int Health = 15;

        States _state = States.Normal;
        float _timer = 0;

        SpriteAnimator _animator;

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponent<SpriteAnimator>();
        }

        public void OnHurt(Hurtable hurtable, Collider attacker)
        {
            if (Health > 0)
            {
                var damage = attacker.GetComponent<Damage>();
                Health -= damage?.Amount ?? 1;
                if (Health <= 0)
                {
                    SetState(States.Dead);
                    Entity.RemoveComponent(hurtable);
                }
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
                _animator.Change("dead", SpriteAnimator.LoopMode.ClampForever);
                //if (Timer.OnTime(_timer, 2f))
                //    Entity.Destroy();
            }
        }

        void SetState(States state)
        {
            _state = state;
            _timer = 0;
        }
    }
}
