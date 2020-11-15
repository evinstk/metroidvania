using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace Game
{
    class MobMover : Component, IUpdatable
    {
        public int MoveSpeed = 600;
        public float Gravity = 3000;
        public float JumpHeight = 32 * 5;
        public float JumpDuration = 0.2f;
        public float MaxY = 1000f;

        public Vector2 Velocity => _velocity;
        Vector2 _velocity;
        float _jumpElapsed = 0;
        public int Facing = 1;

        CollisionComponent _collision;
        ControllerComponent _controller;
        TiledMapMover _mover;
        ColliderTriggerHelper _triggerHelper;
        BoxCollider _boxCollider;

        public bool AttackInput => _controller.AttackPressed;

        public MobMover(BoxCollider boxCollider)
        {
            _boxCollider = boxCollider;
            Insist.IsNotNull(_boxCollider);
        }

        public override void OnAddedToEntity()
        {
            _collision = Entity.GetComponent<CollisionComponent>();
            Insist.IsNotNull(_collision);
            _controller = Entity.GetComponent<ControllerComponent>();
            Insist.IsNotNull(_controller);
            _mover = Entity.GetComponent<TiledMapMover>();
            Insist.IsNotNull(_mover);
            _triggerHelper = new ColliderTriggerHelper(Entity);
        }

        public void Update()
        {
            var moveDir = new Vector2(_controller.XAxis, 0);

            _velocity.X = MoveSpeed * moveDir.X;
            if (_velocity.X != 0)
            {
                Facing = _velocity.X > 0 ? 1 : -1;
            }

            if (_collision.Collision.Below && _controller.JumpPressed)
            {
                _velocity.Y = -Mathf.Sqrt(2f * JumpHeight * Gravity);
                _jumpElapsed = 0;
            }

            if (!_controller.JumpDown && _velocity.Y < 0)
            {
                _velocity.Y = 0;
            }

            _jumpElapsed += Time.DeltaTime;
            if (_velocity.Y > 0 || !_controller.JumpDown || _jumpElapsed > JumpDuration)
            {
                _velocity.Y += Gravity * Time.DeltaTime;
            }

            _velocity.Y = Mathf.Clamp(_velocity.Y, -MaxY, MaxY);

            _mover.Move(_velocity * Time.DeltaTime, _boxCollider, _collision.Collision);
            _triggerHelper.Update();

            if (_collision.Collision.Below)
            {
                _velocity.Y = 0;
            }
        }
    }
}
