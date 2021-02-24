using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace Game
{
    class Sentry : Component, IUpdatable
    {
        public int CastDistance = 200;
        public float AttackInterval = 1f;
        public float ProjectileSpeed = 200f;
        public float ProjectileTimeToLive = 2f;
        public Collider Hitbox;

        enum States
        {
            Normal,
            Turtle,
            Attack,
            Dead,
        }

        public int Health = 15;

        States _state = States.Normal;
        float _timer = 0;
        int _facing = 1;

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
                    Hitbox.PhysicsLayer &= ~Mask.Enemy;
                    Entity.RemoveComponent(hurtable);
                }
            }
        }

        public void Update()
        {
            _timer += Time.DeltaTime;
            _animator.FlipX = _facing != 1;

            // NORMAL STATE
            if (_state == States.Normal)
            {
                // do two directions
                for (var i = -1; i <= 1; i += 2)
                {
                    if (FindTarget(i))
                    {
                        _facing = i;
                        SetState(States.Turtle);
                        break;
                    }
                }
            }
            // TURTLE STATE
            else if (_state == States.Turtle)
            {
                _animator.Change("turtle", SpriteAnimator.LoopMode.ClampForever);
                if (!_animator.IsRunning)
                {
                    SetState(States.Attack);
                }
            }
            // ATTACK STATE
            else if (_state == States.Attack)
            {
                if (_timer >= AttackInterval)
                {
                    // create projectile
                    {
                        var projectileEntity = Entity.Scene.CreateProjectile(Entity.Position + new Vector2(4 * _facing, -11));
                        var mover = projectileEntity.GetComponent<PlatformerMover>();
                        mover.Speed = new Vector2(_facing * ProjectileSpeed, 0);
                        var projectile = projectileEntity.GetComponent<Projectile>();
                        projectile.TimeToLive = ProjectileTimeToLive;
                    }
                    _timer = 0;
                }
            }
            // DEAD STATE
            else if (_state == States.Dead)
            {
                _animator.Change("dead", SpriteAnimator.LoopMode.ClampForever);
                //if (Timer.OnTime(_timer, 2f))
                //    Entity.Destroy();
            }
        }

        bool FindTarget(int dir)
        {
            var pos = Entity.Position;
            var hit = Physics.Linecast(pos, pos + new Vector2(dir * CastDistance, 0), Mask.Player | Mask.Terrain);
            return (hit.Collider?.PhysicsLayer & Mask.Player) > 0;
        }

        void SetState(States state)
        {
            _state = state;
            _timer = 0;
        }
    }
}
