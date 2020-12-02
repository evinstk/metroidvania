﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using Nez.Tiled;
using System;
using System.Linq;

namespace Game.MobState
{
    class MobStateMachine : Component, IUpdatable
    {
        public int MoveSpeed = 600;
        public float Gravity = 3000;
        public float JumpVelocity = 980f;
        public float MaxFallVelocity = 1000f;
        public float JumpDuration = 0.2f;

        public int Facing { get; private set; } = 1;

        Vector2 _velocity;
        float _jumpElapsed = 0;

        StateMachine<MobStateMachine> _fsm;

        BoxCollider _boxCollider;
        CollisionComponent _collision;
        ControllerComponent _controller;
        TiledMapMover _mover;
        Animator<Frame> _animator;

        public MobStateMachine(BoxCollider boxCollider)
        {
            _boxCollider = boxCollider;
        }

        public MobStateMachine SetMoveSpeed(int moveSpeed)
        {
            MoveSpeed = moveSpeed;
            return this;
        }

        public override void OnAddedToEntity()
        {
            _collision = Entity.GetComponent<CollisionComponent>();
            _controller = Entity.GetComponent<ControllerComponent>();
            _mover = Entity.GetComponent<TiledMapMover>();
            _animator = Entity.GetComponent<Animator<Frame>>();

            // assume on ground
            _collision.Collision.Below = true;

            _fsm = new StateMachine<MobStateMachine>(this, new GroundState());
            _fsm.AddState(new AttackState());
            _fsm.AddState(new AirState());
            //_fsm = new StateMachine<MobStateMachine>(this, new IdleState());
            //_fsm.AddState(new WalkState());
        }

        public void Update()
        {
            _fsm.Update(Time.DeltaTime);
        }

        void ChangeAnimation(string animationPrefix, Animator<Frame>.LoopMode loopMode = Animator<Frame>.LoopMode.Loop)
        {
            var animation = animationPrefix + (Facing >= 0 ? "Right" : "Left");
            if (_animator.CurrentAnimationName != animation)
            {
                _animator.Play(animation, loopMode);
            }
        }

        void Move(float deltaTime)
        {
            _mover.Move(_velocity * deltaTime, _boxCollider, _collision.Collision);
        }

        void GroundOrAir(string animation)
        {
            if (_collision.Collision.Below)
            {
                _fsm.ChangeState<GroundState>();
            }
            else
            {
                _fsm.ChangeState<AirState>();
            }
        }

        class GroundState : State<MobStateMachine>
        {
            public override void Begin()
            {
                _context.ChangeAnimation("Idle");
                _context._velocity.Y = 0;
            }

            public override void Reason()
            {
                if (!_context._collision.Collision.Below)
                {
                    _machine.ChangeState<AirState>();
                }
                if (_context._controller.AttackPressed)
                {
                    _machine.ChangeState<AttackState>();
                }
            }

            public override void Update(float deltaTime)
            {
                _context._velocity.Y += _context.Gravity * deltaTime; // ensure collision below

                var moveDir = new Vector2(_context._controller.XAxis, 0);

                if (moveDir.X == 0)
                {
                    _context.ChangeAnimation("Idle");
                }
                else
                {
                    _context.ChangeAnimation("Walk");
                    _context.Facing = Math.Sign(moveDir.X);
                }
                _context._velocity.X = _context.MoveSpeed * moveDir.X;

                if (_context._controller.JumpPressed)
                {
                    _context._velocity.Y = -_context.JumpVelocity;
                }

                _context.Move(deltaTime);

                if (_context._collision.Collision.Below)
                {
                    _context._velocity.Y = 0;
                }
            }
        }

        class AttackState : State<MobStateMachine>
        {
            public override void Begin()
            {
                _context.ChangeAnimation("Attack", Animator<Frame>.LoopMode.Once);
                _context._animator.OnAnimationCompletedEvent += _context.GroundOrAir;
            }

            public override void End()
            {
                _context._animator.OnAnimationCompletedEvent -= _context.GroundOrAir;
            }

            public override void Update(float deltaTime)
            {
                _context._velocity.Y += _context.Gravity * deltaTime; // ensure collision below
                var moveDir = new Vector2(_context._controller.XAxis, 0);
                _context._velocity.X = _context.MoveSpeed * moveDir.X;
                _context.Move(deltaTime);
            }
        }

        class AirState : State<MobStateMachine>
        {
            public override void Begin()
            {
                _context.ChangeAnimation(_context._velocity.Y >= 0 ? "Fall" : "Jump", Animator<Frame>.LoopMode.ClampForever);
                _context._jumpElapsed = 0;
            }

            public override void Reason()
            {
                if (_context._controller.AttackPressed)
                {
                    _machine.ChangeState<AttackState>();
                }
                if (_context._collision.Collision.BecameGroundedThisFrame)
                {
                    _context.ChangeAnimation("Land", Animator<Frame>.LoopMode.ClampForever);
                    _context._animator.OnAnimationCompletedEvent += _context.GroundOrAir;
                }
            }

            public override void Update(float deltaTime)
            {
                _context._jumpElapsed += deltaTime;

                // cancel upward motion if jump released or collision above
                if (_context._velocity.Y < 0 &&
                    (!_context._controller.JumpDown)
                     //|| _context._jumpElapsed >= _context.JumpDuration)
                    || _context._collision.Collision.Above)
                {
                    _context._velocity.Y = 0;
                }
                // apply gravity if already falling, not holding jump, or jump duration exceeded
                if (_context._velocity.Y >= 0 || !_context._controller.JumpDown || _context._jumpElapsed >= _context.JumpDuration)
                {
                    _context._velocity.Y += _context.Gravity * deltaTime;
                }
                _context._velocity.Y = Mathf.Clamp(_context._velocity.Y, -_context.JumpVelocity, _context.MaxFallVelocity);

                var moveDir = new Vector2(_context._controller.XAxis, 0);
                if (moveDir.X != 0)
                {
                    _context.Facing = Math.Sign(moveDir.X);
                }
                _context._velocity.X = _context.MoveSpeed * moveDir.X;
                _context.Move(deltaTime);

                if (!_context._animator.CurrentAnimationName.Contains("Land"))
                {
                    _context.ChangeAnimation(_context._velocity.Y >= 0 ? "Fall" : "Jump", Animator<Frame>.LoopMode.ClampForever);
                }
            }

            public override void End()
            {
                _context._animator.OnAnimationCompletedEvent -= _context.GroundOrAir;
            }
        }
    }
}
