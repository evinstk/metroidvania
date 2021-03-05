using FMOD.Studio;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class MechBoss : Component, IUpdatable, IHealthy
    {
        public float MoveSpeed = 50f;

        public float LiftoffSpeed = 150f;
        public float LiftoffHeight = 160f;

        public float VerticalFlySpeed = 20f;
        public float HorizontalFlySpeed = 150f;
        public float PeriodDuration = 2f;

        public float FlyAttackInterval = 0.4f;
        public float FlyAttackProjectileSpeed = 200f;
        public Vector2[] FlyAttackOffset = new Vector2[]
        {
            new Vector2(18, 23),
            new Vector2(-14, 23),
        };

        public float SlamGravity = 800f;

        public Vector2[] FireAttackOffset = new Vector2[]
        {
            new Vector2(33, 9),
            new Vector2(-1, 9),
        };

        public Collider Hitbox;

        public EventInstance FireSound;
        public EventInstance DeathSound;

        enum States
        {
            Start,
            Normal,
            Liftoff,
            Fly,
            Slam,
            Fire,
            Dead,
        }

        States _state = States.Start;
        public int Health { get; set; } = 75;
        int _facing = 1;
        float _timer = 0;

        SpriteAnimator _animator;
        PlatformerMover _mover;

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponent<SpriteAnimator>();
            _mover = Entity.GetComponent<PlatformerMover>();
        }

        public void Begin()
        {
            SetState(States.Normal);
        }

        public void Update()
        {
            _timer += Time.DeltaTime;

            var position = Entity.Position;
            _animator.FlipX = _facing < 1;
            var player = Entity.Scene.FindEntity("player");

            if (_state == States.Start)
            {
                _facing = Math.Sign(player.Position.X - position.X);
            }
            else if (_state == States.Normal)
            {
                if (_mover.OnWall(out var dir) && dir == _facing)
                    _facing = _facing * -1;
                _mover.Speed.X = MoveSpeed * _facing;
                _animator.Change(_mover.Speed.X != 0 ? "walk" : "idle");

                if (_timer > 2f)
                {
                    var randState = Calc.RandInt(2);
                    if (randState == 0)
                        SetState(States.Fire);
                    else
                        SetState(States.Liftoff);
                }
            }
            else if (_state == States.Liftoff)
            {
                if (Timer.OnTime(_timer, 0.01f))
                {
                    _animator.Change("fire", SpriteAnimator.LoopMode.ClampForever);
                    _mover.Speed.X = 0;
                }
                else if (!_animator.IsRunning)
                {
                    _animator.Change("fly");
                    _mover.Speed.X = 0;
                    _mover.Speed.Y = -LiftoffSpeed;
                }

                // high enough?
                var hit = Physics.Linecast(position, position + new Vector2(0, LiftoffHeight), Mask.Terrain);
                if (hit.Collider == null)
                    SetState(States.Fly);
            }
            else if (_state == States.Fly)
            {
                _animator.Change("fly");
                _mover.Speed.Y = Mathf.Sin(2f * MathHelper.Pi * (_timer % PeriodDuration) / PeriodDuration) * VerticalFlySpeed;
                if (_mover.OnWall(out var dir) && dir == _facing)
                    _facing = _facing * -1;
                _mover.Speed.X = HorizontalFlySpeed * _facing;

                //if (Timer.OnTime(_timer % FlyAttackInterval, 0))
                if (Timer.OnInterval(FlyAttackInterval))
                {
                    for (var i = 0; i < FlyAttackOffset.Length; ++i)
                    {
                        var attackOffset = FlyAttackOffset[i];
                        attackOffset.X *= _facing;
                        var projectilePos = position + attackOffset;
                        //_gunIndex = (_gunIndex + 1) % FlyAttackOffset.Length;
                        Entity.Scene.CreateProjectile(
                            projectilePos,
                            new Vector2(0, FlyAttackProjectileSpeed),
                            "large_laser",
                            Mask.EnemyAttack,
                            Mask.Terrain | Mask.Player);
                    }
                    FireSound.start();
                }
                if (_timer > 3f)
                    SetState(States.Slam);
            }
            else if (_state == States.Slam)
            {
                _animator.Change("fire", SpriteAnimator.LoopMode.ClampForever);
                _mover.Speed.X = 0;
                _mover.Speed.Y += SlamGravity * Time.DeltaTime;

                if (_timer > 1.5f)
                    SetState(States.Normal);
            }
            else if (_state == States.Fire)
            {
                _facing = Math.Sign(player.Position.X - position.X);
                _animator.Change("fire", SpriteAnimator.LoopMode.ClampForever);
                _mover.Speed.X = 0;

                var startFire = 0.5f;
                if (Timer.OnTime(_timer, startFire) || Timer.OnTime(_timer, startFire + 0.3f))
                {
                    for (var i = 0; i < FireAttackOffset.Length; ++i)
                    {
                        var attackOffset = FireAttackOffset[i];
                        attackOffset.X *= _facing;
                        var projectilePos = position + attackOffset;
                        Entity.Scene.CreateProjectile(
                            projectilePos,
                            new Vector2(FlyAttackProjectileSpeed * _facing, 0),
                            "large_laser",
                            Mask.EnemyAttack,
                            Mask.Terrain | Mask.Player);
                    }
                    FireSound.start();
                }
                if (_timer >= 2f)
                    SetState(States.Normal);
            }
            else if (_state == States.Dead)
            {
                if (_timer < 3f)
                {
                    _animator.Change("fire", SpriteAnimator.LoopMode.ClampForever);
                    _mover.Speed.X = 0;
                    _mover.Speed.Y += Time.DeltaTime * SlamGravity;

                    if (Timer.OnInterval(0.25f))
                    {
                        var offset = new Vector2(Calc.RandInt(-32, 32), Calc.RandInt(-8, 8));
                        Entity.Scene.CreateBoom(position + offset);
                        DeathSound.start();
                    }
                }
                else if (Timer.OnTime(_timer, 3f))
                {
                    _animator.Change("dead", SpriteAnimator.LoopMode.ClampForever);
                    DeathSound.start();
                    for (int x = -1; x < 2; ++x)
                        for (int y = -1; y < 2; ++y)
                            Entity.Scene.CreateBoom(position + new Vector2(x * 12, -2 + y * 12));
                }
            }
        }

        void SetState(States state)
        {
            _state = state;
            _timer = 0;
        }

        public void OnHurt(Hurtable self, Collider attacker)
        {
            var damage = attacker.GetDamageAmount();
            Health -= damage;
            if (Health <= 0)
            {
                Entity.RemoveComponent(self);
                Hitbox.PhysicsLayer &= ~Mask.EnemyAttack;
                SetState(States.Dead);
            }
        }
    }
}
