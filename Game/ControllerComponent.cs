using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tiled;
using static Nez.Tiled.TiledMapMover;

namespace Game
{
    class ControllerComponent : Component, IUpdatable
    {
        public int MoveSpeed = 450;
        public float Gravity = 1000;
        public float JumpHeight = 32 * 5;

        public Vector2 Velocity => _velocity;
        Vector2 _velocity;
        public int Facing = 1;

        public CollisionState Collision { get; } = new CollisionState();
        TiledMapMover _mover;
        BoxCollider _boxCollider;

        VirtualAxis _xAxisInput;
        VirtualButton _jumpInput;
        public bool AttackInput => _attackInput.IsPressed;
        VirtualButton _attackInput;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<TiledMapMover>();
            Insist.IsNotNull(_mover);
            _boxCollider = Entity.GetComponent<BoxCollider>();
            Insist.IsNotNull(_boxCollider);

            _xAxisInput = new VirtualAxis();
            _xAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D));
            _xAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right));
            _xAxisInput.Nodes.Add(
                new VirtualAxis.GamePadLeftStickX());

            _jumpInput = new VirtualButton();
            _jumpInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));
            _jumpInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));

            _attackInput = new VirtualButton();
            _attackInput.Nodes.Add(new VirtualButton.MouseLeftButton());
            _attackInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.X));
        }

        public void Update()
        {
            var moveDir = new Vector2(_xAxisInput.Value, 0);

            _velocity.X = MoveSpeed * moveDir.X;
            if (_velocity.X != 0)
            {
                Facing = _velocity.X > 0 ? 1 : -1;
            }

            if (Collision.Below && _jumpInput.IsPressed)
            {
                _velocity.Y = -Mathf.Sqrt(2f * JumpHeight * Gravity);
            }

            if (!_jumpInput.IsDown && _velocity.Y < 0)
            {
                _velocity.Y = 0;
            }

            _velocity.Y += Gravity * Time.DeltaTime;

            _mover.Move(_velocity * Time.DeltaTime, _boxCollider, Collision);

            if (Collision.Below)
            {
                _velocity.Y = 0;
            }
        }
    }
}
