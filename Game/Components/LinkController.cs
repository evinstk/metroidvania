using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using System;

namespace Game.Components
{
    class LinkController : Component, IUpdatable
    {
        enum States
        {
            Normal,
            Attack,
        }

        public string AnimationIdle;
        public string AnimationWalk;
        public string AnimationRun;
        public string AnimationAttack;

        public float Gravity = 600f;
        public float MoveSpeed = 150f;
        public float JumpTime = .4f;
        public float JumpSpeed = 200f;
        public float MaxFallSpeed = 200f;

        States _state = States.Normal;

        PlatformerMover _mover;
        SpriteAnimator _anim;

        VirtualJoystick _inputMovement;
        VirtualButton _inputJump;
        VirtualButton _inputAttack;

        int _facing = 1;
        float _jumpTimer;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<PlatformerMover>();
            _anim = Entity.GetComponent<SpriteAnimator>();

            _inputMovement = new VirtualJoystick(false);
            _inputMovement.AddGamePadLeftStick();
            _inputMovement.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D, Keys.W, Keys.S);
            _inputMovement.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right, Keys.Up, Keys.Down);

            _inputJump = new VirtualButton();
            _inputJump.AddGamePadButton(0, Buttons.A);
            _inputJump.AddKeyboardKey(Keys.Space);

            _inputAttack = new VirtualButton();
            _inputAttack.AddGamePadButton(0, Buttons.X);
            _inputAttack.AddMouseLeftButton();
        }

        public void Update()
        {
            var onGround = _mover.OnGround();
            var inputX = _inputMovement.Value.X;
            var halfSpeed = Math.Abs(inputX) < .5f;

            // NORMAL STATE
            if (_state == States.Normal)
            {
                // horizontal movement
                {
                    var dir = Math.Sign(inputX);
                    var multiplier = halfSpeed ? .5f : 1;
                    _mover.Speed.X = dir * multiplier * MoveSpeed;
                    if (inputX != 0)
                        _facing = dir;
                }

                // invoke jumping
                {
                    if (_inputJump.IsPressed && onGround)
                    {
                        _jumpTimer = JumpTime;
                    }
                }

                // invoke attacking
                {
                    if (_inputAttack.IsPressed)
                    {
                        _state = States.Attack;
                    }
                }

                // animation
                var flip = _facing == -1;
                _anim.FlipX = flip;
                if (inputX != 0)
                {
                    if (halfSpeed && AnimationWalk != null)
                        _anim.Change(AnimationWalk);
                    else if (AnimationRun != null)
                        _anim.Change(AnimationRun);
                }
                else if (AnimationIdle != null)
                {
                    _anim.Change(AnimationIdle);
                }
            }
            // ATTACK STATE
            else if (_state == States.Attack)
            {
                _anim.Change(AnimationAttack, SpriteAnimator.LoopMode.ClampForever);

                if (onGround)
                    _mover.Speed.X = 0;

                if (!_anim.IsRunning)
                {
                    _state = States.Normal;
                }
            }

            // gravity
            if (!onGround)
            {
                _mover.Speed.Y += Gravity * Time.DeltaTime;
            }

            // jumping
            if (_jumpTimer > 0)
            {
                _jumpTimer -= Time.DeltaTime;
                _mover.Speed.Y = -JumpSpeed;
                if (!_inputJump.IsDown || _mover.OnGround(-1) || _jumpTimer <= 0)
                {
                    _jumpTimer = 0;
                    _mover.Speed.Y = 0;
                }
            }

            _mover.Speed.Y = Mathf.Clamp(_mover.Speed.Y, -JumpSpeed, MaxFallSpeed);
        }
    }
}
