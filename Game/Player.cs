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

        public float MoveSpeed = 100f;

        States _state = States.Normal;
        VirtualIntegerAxis _inputX;
        PlatformerMover _mover;
        SpriteAnimator _animator;
        int _facing = 1;

        bool _onGround = false;

        public override void OnAddedToEntity()
        {
            _inputX = new VirtualIntegerAxis();
            _inputX.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D));
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right));

            _mover = Entity.GetComponent<PlatformerMover>();
            _animator = Entity.GetComponent<SpriteAnimator>();
        }

        public void Update()
        {
            _animator.FlipX = _facing == -1;
            _onGround = _mover.OnGround();
            Debug.Log(_onGround);

            if (_state == States.Normal)
            {
                var inputX = _inputX.Value;

                if (inputX == 0)
                    _animator.Change("idle");
                else
                    _animator.Change("walk");

                _mover.Speed.X = inputX * MoveSpeed;

                if (inputX != 0)
                    _facing = inputX;
            }
        }
    }
}
