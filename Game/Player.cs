using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace Game
{
    class Player : Component, IUpdatable
    {
        enum States
        {
            Normal
        }

        public float MoveSpeed = 150f;
        public float Gravity = 600f;
        public float JumpTime = .4f;
        public float JumpSpeed = 200f;
        public float MaxFallSpeed = 200f;

        States _state = States.Normal;
        int _facing = 1;
        float _jumpTimer = 0;

        VirtualIntegerAxis _inputX;
        VirtualButton _inputJump;

        PlatformerMover _mover;
        SpriteAnimator _animator;

        bool _onGround = false;

        public override void OnAddedToEntity()
        {
            _inputX = new VirtualIntegerAxis();
            _inputX.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D));
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right));

            _inputJump = new VirtualButton();
            _inputJump.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
            _inputJump.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));

            _mover = Entity.GetComponent<PlatformerMover>();
            _animator = Entity.GetComponent<SpriteAnimator>();
        }

        public void Update()
        {
            _animator.FlipX = _facing == -1;
            _onGround = _mover.OnGround();
            var inputX = _inputX.Value;
            Debug.Log(_onGround);

            if (_state == States.Normal)
            {
                // animation
                if (inputX == 0)
                    _animator.Change("idle");
                else
                    _animator.Change("walk");

                // horizontal movement
                {
                    _mover.Speed.X = inputX * MoveSpeed;

                    if (inputX != 0 && _onGround)
                        _facing = inputX;
                }

                // invoke jumping
                {
                    if (_inputJump.IsPressed && _onGround)
                    {
                        _inputJump.ConsumeBuffer();
                        _jumpTimer = JumpTime;
                    }
                }
            }

            // gravity
            if (!_onGround)
            {
                _mover.Speed.Y += Gravity * Time.DeltaTime;
            }

            // jumping
            if (_jumpTimer > 0)
            {
                _mover.Speed.Y = -JumpSpeed;
                _jumpTimer -= Time.DeltaTime;
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
