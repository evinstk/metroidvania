using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;

namespace Game
{
    class CutsceneController : Component, IUpdatable
    {
        enum States
        {
            Free,
            Move,
        }

        public Action<CutsceneController> OnPossess;
        public Action<CutsceneController> OnRelease;
        public float WalkSpeed = 50f;
        public float Gravity = 600f;

        States _state = States.Free;
        int _facing = 1;
        Vector2 _moveDest;

        List<SpriteAnimator> _animators;
        PlatformerMover _mover;

        public void Move(Vector2 dest)
        {
            _moveDest = dest;
            _state = States.Move;
        }

        public override void Initialize()
        {
            Enabled = false;
        }

        public void Possess()
        {
            OnPossess?.Invoke(this);
            ResetMovement();
            Enabled = true;
        }

        public void Release()
        {
            OnRelease?.Invoke(this);
            Enabled = false;
        }

        public override void OnAddedToEntity()
        {
            _animators = Entity.GetComponents<SpriteAnimator>();
            _mover = Entity.GetComponent<PlatformerMover>();
        }

        void ResetMovement()
        {
            // OnAddedToEntity may not be called yet
            var mover = Entity.GetComponent<PlatformerMover>();
            mover.Speed.X = 0;
            mover.Speed.Y = 0;
            var anim = Entity.GetComponent<SpriteAnimator>();
            if (anim.Animations.ContainsKey("idle"))
                anim.Change("idle");
        }

        public void Update()
        {
            foreach (var animator in _animators)
                animator.FlipX = _facing < 0;

            // MOVE STATE
            if (_state == States.Move)
            {
                var pos = Entity.Position;
                if (Math.Abs(_moveDest.X - pos.X) > 0.001f)
                {
                    foreach (var animator in _animators)
                        if (animator.Animations.ContainsKey("walk"))
                            animator.Change("walk");
                    _facing = Math.Sign(_moveDest.X - pos.X);
                    _mover.Speed.X = WalkSpeed * _facing;
                }
                else
                {
                    foreach (var animator in _animators)
                        if (animator.Animations.ContainsKey("idle"))
                            animator.Change("idle");
                    _mover.Speed.X = 0;
                    _state = States.Free;
                }
            }

            if (!_mover.OnGround())
                _mover.Speed.Y += Gravity * Time.DeltaTime;
        }
    }
}
