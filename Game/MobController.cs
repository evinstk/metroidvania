using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class MobController : ControllerComponent, IUpdatable
    {
        public int LinecastDistance = 150;
        public float AttackTimeout = 1f;

        public override float XAxis => _attackTimeoutActive ? 0 : _dir;
        float _dir = 1f;
        public override bool AttackPressed => _attack;
        bool _attack = false;
        bool _attackTimeoutActive = false;

        CollisionComponent _collision;

        public override void OnAddedToEntity()
        {
            _collision = Entity.GetComponent<CollisionComponent>();
            Insist.IsNotNull(_collision);
        }

        public void Update()
        {
            _attack = false;

            if (_collision.Collision.Right || _collision.Collision.Left)
                _dir = -_dir;
            if (Entity.Position.X <= 0)
                _dir = 1f;

            var hit = Physics.Linecast(
                Entity.Position,
                Entity.Position + new Vector2(LinecastDistance * _dir, 0),
                Layer.HurtBox);
            if (hit.Collider != null && !_attackTimeoutActive)
            {
                _attack = _attackTimeoutActive = true;
                Core.Schedule(AttackTimeout, timer => _attackTimeoutActive = false);
            }
        }
    }
}
