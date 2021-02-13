using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace Game
{
    class Player : Component, IUpdatable
    {
        enum States
        {
            Normal,
            Attack,
        }

        public float MoveSpeed = 150f;
        public float Gravity = 600f;
        public float JumpTime = .4f;
        public float JumpSpeed = 200f;
        public float MaxFallSpeed = 200f;

        States _state = States.Normal;
        int _facing = 1;
        float _jumpTimer = 0;
        float _attackTimer = 0;
        BoxCollider _attackCollider;

        VirtualIntegerAxis _inputX;
        VirtualButton _inputJump;
        VirtualButton _inputAttack;

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

            _inputAttack = new VirtualButton();
            _inputAttack.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.X));
            _inputAttack.Nodes.Add(new VirtualButton.MouseLeftButton());

            _mover = Entity.GetComponent<PlatformerMover>();
            _animator = Entity.GetComponent<SpriteAnimator>();
        }

        public void Update()
        {
            _animator.FlipX = _facing == -1;
            _onGround = _mover.OnGround();
            var inputX = _inputX.Value;

            // NORMAL STATE
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

                // invoke attacking
                {
                    if (_inputAttack.IsPressed)
                    {
                        _inputAttack.ConsumeBuffer();
                        _state = States.Attack;
                        _attackTimer = 0;

                        if (_attackCollider == null)
                            _attackCollider = Entity.AddComponent(new BoxCollider(0, 0));
                        _attackCollider.PhysicsLayer = Mask.PlayerAttack;

                        if (_onGround)
                            _mover.Speed.X = 0;
                    }
                }
            }
            // ATTACK STATE
            else if (_state == States.Attack)
            {
                _animator.Change("attack", SpriteAnimator.LoopMode.ClampForever);
                _attackTimer += Time.DeltaTime;

                if (_attackTimer > .08f && _attackTimer < .2f)
                {
                    _attackCollider.SetLocalOffset(new Vector2(16, 0));
                    _attackCollider.SetSize(32, 32);
                }
                else if (_attackTimer > .08f && _attackCollider != null)
                {
                    Entity.RemoveComponent(_attackCollider);
                    _attackCollider = null;
                }

                if (_facing < 0 && _attackCollider != null)
                {
                    var offset = _attackCollider.LocalOffset;
                    _attackCollider.LocalOffset = new Vector2(-offset.X, offset.Y);
                }

                if (!_animator.IsRunning)
                {
                    _animator.Change("idle");
                    _state = States.Normal;
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
