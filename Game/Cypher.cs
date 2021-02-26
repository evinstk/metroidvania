using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;

namespace Game
{
    class Cypher : Component, IUpdatable
    {
        public Collider Hitbox;
        public float Gravity = 600f;
        public float MaxFallSpeed = 300f;
        public int Health = 5;
        public SoundEffect DeathSound;

        enum States
        {
            Normal,
            Dead,
        }

        States _state = States.Normal;

        SpriteAnimator _animator;
        PlatformerMover _mover;

        public void OnHurt(Hurtable self, Collider attacker)
        {
            var damage = attacker.GetComponent<Damage>();
            Health -= damage?.Amount ?? 1;
            if (Health <= 0)
            {
                DeathSound.Play();
                SetState(States.Dead);
                Entity.RemoveComponent(self);
            }
        }

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponent<SpriteAnimator>();
            _mover = Entity.GetComponent<PlatformerMover>();
        }

        public void Update()
        {
            if (_state == States.Normal)
            {
                _animator.Change("idle");
            }
            else if (_state == States.Dead)
            {
                _animator.Change("dead");
                Hitbox.PhysicsLayer &= ~(Mask.EnemyAttack | Mask.Enemy);
                _mover.Speed.Y += Gravity * Time.DeltaTime;
            }

            _mover.Speed.Y = Mathf.Clamp(_mover.Speed.Y, 0, MaxFallSpeed);
        }

        void SetState(States state)
        {
            _state = state;
        }
    }
}
