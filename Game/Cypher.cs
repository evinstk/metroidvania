using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class Cypher : Component, IUpdatable
    {
        public Collider Hitbox;

        public float PatrolRange = 200f;
        public float PatrolSpeed = 80f;
        public float VerticalSpeed = 40f;
        public float PeriodDuration = 2f;
        public float AttackTimeoutDuration = 1f;
        public float AttackAngle = 45f;
        public float ProjectileSpeed = 150f;
        public float KnockbackDuration = 0.25f;
        public float KnockbackSpeed = 150f;
        public int StartDir = 1;
        public Vector2 CheckOffset = new Vector2(100f);
        public float Gravity = 600f;
        public float MaxFallSpeed = 300f;
        public int Health = 9;

        public FMOD.Studio.EventInstance FireSound;
        public FMOD.Studio.EventInstance DeathSound;

        enum States
        {
            Normal,
            Dead,
        }

        States _state = States.Normal;
        Vector2 _startPos;
        float _periodTimer;
        float _attackTimerDuration;
        float _knockbackTimer;
        int _knockbackDir;
        int _facing = 0;

        SpriteAnimator _animator;
        PlatformerMover _mover;
        SpriteRenderer _renderer;

        public void OnHurt(Hurtable self, Collider attacker)
        {
            var damage = attacker.GetComponent<Damage>();
            Health -= damage?.Amount ?? 1;

            _knockbackTimer = KnockbackDuration;
            _knockbackDir = Math.Sign(Entity.Position.X - attacker.AbsolutePosition.X);

            if (Health <= 0)
            {
                DeathSound.start();
                SetState(States.Dead);
                Entity.RemoveComponent(self);
            }
        }

        public override void OnAddedToEntity()
        {
            _startPos = Entity.Position;
            _facing = StartDir;

            _animator = Entity.GetComponent<SpriteAnimator>();
            _mover = Entity.GetComponent<PlatformerMover>();
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public void Update()
        {
            var currPos = Entity.Position;
            _renderer.FlipX = _facing < 0;

            if (_state == States.Normal)
            {
                _animator.Change("idle");
                _mover.Speed.X = PatrolSpeed * _facing;
                _mover.Speed.Y = Mathf.Sin(2f * MathHelper.Pi * _periodTimer / PeriodDuration) * VerticalSpeed;

                var diff = _startPos.X - currPos.X;
                if (Math.Abs(diff) * 2 >= PatrolRange)
                {
                    _facing = Math.Sign(diff);
                }

                if (_attackTimerDuration <= 0 && Physics.OverlapCircle(currPos + new Vector2(CheckOffset.X * _facing, CheckOffset.Y), 128f, Mask.Player) != null)
                {
                    _attackTimerDuration = AttackTimeoutDuration;

                    var projectilePos = currPos + new Vector2(6 * _facing, 13);
                    Entity.Scene.CreateFlash(projectilePos, Color.AliceBlue);
                    var projectileEntity = Entity.Scene.CreateProjectile(projectilePos);
                    var mover = projectileEntity.GetComponent<PlatformerMover>();
                    mover.Speed = Mathf.RotateAround(new Vector2(_facing, 0), Vector2.Zero, AttackAngle * _facing) * ProjectileSpeed;

                    FireSound.start();
                }
            }
            else if (_state == States.Dead)
            {
                _animator.Change("dead");
                Hitbox.PhysicsLayer &= ~(Mask.EnemyAttack | Mask.Enemy);
                if (_mover.OnGround())
                    _mover.Speed.X = 0;
                _mover.Speed.Y += Gravity * Time.DeltaTime;
            }

            // period timer
            _periodTimer += Time.DeltaTime;
            if (_periodTimer >= PeriodDuration)
            {
                _periodTimer -= PeriodDuration;
            }

            // attack timeout timer
            if (_attackTimerDuration > 0)
            {
                _attackTimerDuration -= Time.DeltaTime;
            }

            // knockback
            if (_knockbackTimer > 0)
            {
                _knockbackTimer -= Time.DeltaTime;
                _mover.Speed.X = _knockbackDir * KnockbackSpeed;
            }

            _mover.Speed.Y = Mathf.Clamp(_mover.Speed.Y, -VerticalSpeed, MaxFallSpeed);
        }

        void SetState(States state)
        {
            _state = state;
        }
    }
}
