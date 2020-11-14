using Nez;

namespace Game
{
    class MobController : ControllerComponent, IUpdatable
    {
        public override float XAxis => _dir;
        float _dir = 1f;

        CollisionComponent _collision;

        public override void OnAddedToEntity()
        {
            _collision = Entity.GetComponent<CollisionComponent>();
            Insist.IsNotNull(_collision);
        }

        public void Update()
        {
            if (_collision.Collision.Right || _collision.Collision.Left)
                _dir = -_dir;
            if (Entity.Position.X <= 0)
                _dir = 1f;
        }
    }
}
