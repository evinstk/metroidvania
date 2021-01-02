using Game.Editor.Animation;
using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;
using System.IO;

namespace Game.Movement
{
    struct MovementAnimation
    {
        public Animation<ObserverFrame> Left;
        public Animation<ObserverFrame> Right;
    }

    partial class PlayerMovement : Component, IUpdatable
    {
        public float Gravity = 300; // acceleration
        public float MoveSpeed = 130;
        public float MaxFallVelocity = 150;
        public float JumpVelocity = 200;
        public float JumpDuration = 0.2f;

        int _facing = 1;

        // animations
        public MovementAnimation Idle;
        public MovementAnimation Walk;

        // external dependencies
        ControllerComponent _controller;

        Microsoft.Xna.Framework.Vector2 _velocity;
        SubpixelFloat _subpixelX;
        SubpixelFloat _subpixelY;
        bool _collisionBelow = true;
        bool _collisionAbove;
        float _jumpElapsed;

        Mover _mover;
        Animator<ObserverFrame> _animator;
        SpriteRenderer _renderer;
        StateMachine<PlayerMovement> _fsm;

        public override void OnAddedToEntity()
        {
            _controller = Entity.GetComponent<ControllerComponent>();
            if (_controller == null)
                _controller = Entity.AddComponent<FreeController>();

            _mover = Entity.AddComponent<Mover>();
            _fsm = new StateMachine<PlayerMovement>(this, new GroundState());
            _fsm.AddState(new AirState());

            _renderer = Entity.GetComponentStrict<SpriteRenderer>();

            _animator = Entity.GetComponentStrict<Animator<ObserverFrame>>();
            _animator.OnFrameEnter += HandleFrameEnter;
            _animator.Play(Idle.Right);

            Entity.GetOrCreateComponent<BoxCollider>();
        }

        void HandleFrameEnter(ObserverFrame frame)
        {
            _renderer.SetSprite(frame.Sprite);
            _renderer.FlipX = frame.Flip;
        }

        public void Update()
        {
            _fsm.Update(Time.DeltaTime);
        }

        void Move(float deltaTime)
        {
            var motion = _velocity * deltaTime;
            _mover.CalculateMovement(ref motion, out var collisionResult);
            _subpixelX.Update(ref motion.X);
            _subpixelY.Update(ref motion.Y);

            _collisionBelow = collisionResult.Normal.Y < 0;
            _collisionAbove = collisionResult.Normal.Y > 0;

            if (collisionResult.Normal.X != 0) _subpixelX.Reset();
            if (collisionResult.Normal.Y != 0) _subpixelY.Reset();

            _mover.ApplyMovement(motion);
        }

        void ChangeAnimation(Animation<ObserverFrame> animation)
        {
            if (!_animator.IsAnimationActive(animation))
                _animator.Play(animation);
        }
    }
}
