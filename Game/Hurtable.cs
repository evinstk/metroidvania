using Nez;
using System;

namespace Game
{
    class Hurtable : Component, IUpdatable
    {
        public float StunTime = .5f;

        public Collider Collider;
        public Action<Hurtable> OnHurt;

        float _stunTimer = 0;

        public void Update()
        {
            if (Collider != null && OnHurt != null && _stunTimer <= 0 && Collider.CollidesWithAny(out _))
            {
                Hurt();
            }

            if (_stunTimer > 0)
                _stunTimer -= Time.DeltaTime;
        }

        void Hurt()
        {
            _stunTimer = StunTime;
            OnHurt(this);
        }
    }
}
