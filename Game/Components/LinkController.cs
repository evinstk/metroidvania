using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace Game.Components
{
    class LinkController : Component, IUpdatable
    {
        public string AnimationIdle;
        public string AnimationWalk;

        public float Gravity = 600f;
        public float MoveSpeed = 150f;
        public float JumpTime = .4f;
        public float JumpSpeed = 200f;

        PlatformerMover _mover;
        SpriteAnimator _anim;

        VirtualIntegerAxis _inputX;
        VirtualButton _inputJump;

        int _facing = 1;
        float _jumpTimer;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<PlatformerMover>();
            _anim = Entity.GetComponent<SpriteAnimator>();

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
                if (inputX != 0)
                    _facing = inputX;
            }

            // invoke jumping
            {
                if (_inputJump.IsPressed && onGround)
                {
                    _jumpTimer = JumpTime;
                }
            }

            // animation
            var flip = _facing == -1;
            _anim.FlipX = flip;
            if (inputX != 0 && AnimationWalk != null)
            {
                _anim.Change(AnimationWalk);
            }
            else if (AnimationIdle != null)
            {
                _anim.Change(AnimationIdle);
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
