using Nez;

namespace Game
{
    class MobController : ControllerComponent, IUpdatable
    {
        public override float XAxis => _dir;
        float _dir = 1f;

        MobMover _mover;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<MobMover>();
            Insist.IsNotNull(_mover);
        }

        public void Update()
        {
            if (_mover.Collision.Right || _mover.Collision.Left)
                _dir = -_dir;
            if (Entity.Position.X <= 0)
                _dir = 1f;
        }
    }
}
