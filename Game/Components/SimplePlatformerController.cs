using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game.Components
{
    class SimplePlatformerController : Component, IUpdatable
    {
        public float Gravity = 600f;
        public float MoveSpeed = 150f;
        public float JumpTime = .4f;
        public float JumpSpeed = 200f;

        PlatformerMover _mover;

        VirtualIntegerAxis _inputX;
        VirtualButton _inputJump;

        float _jumpTimer;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<PlatformerMover>();

            _inputX = new VirtualIntegerAxis();
            _inputX.AddGamePadLeftStickX();
            _inputX.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D);
            _inputX.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right);

            _inputJump = new VirtualButton();
            _inputJump.AddGamePadButton(0, Buttons.A);
            _inputJump.AddKeyboardKey(Keys.Space);
        }

        public void Update()
        {
            var onGround = _mover.OnGround();
            var inputX = _inputX.Value;

            // horizontal movement
            {
                _mover.Speed.X = inputX * MoveSpeed;
            }

            // invoke jumping
            {
                if (_inputJump.IsPressed && onGround)
                {
                    _jumpTimer = JumpTime;
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
        }
    }
}
