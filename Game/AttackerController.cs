using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.BehaviorTrees;

namespace Game
{
    class AttackerController : ControllerComponent, IUpdatable
    {
        public int LinecastDistance = 150;
        public float AttackTimeout = 0.8f;
        public Teams Team
        {
            get => _team;
            set
            {
                _team = value;
                _enemyMask = Layer.GetOtherTeamsMask(value);
            }
        }
        Teams _team = Teams.A;
        int _enemyMask = Layer.GetOtherTeamsMask(Teams.A);

        public override float XAxis => _moving ? _dir : 0;
        float _dir = 1f;
        public override bool AttackPressed => _attack;
        bool _attack = false;
        bool _moving = false;

        CollisionComponent _collision;

        BehaviorTree<AttackerController> _tree;

        public override void OnAddedToEntity()
        {
            _collision = Entity.GetComponent<CollisionComponent>();
            Insist.IsNotNull(_collision);

            var builder = BehaviorTreeBuilder<AttackerController>.Begin(this);

            builder.Sequence()
                .Action(m => m.FindTarget())
                .WaitAction(AttackTimeout)
                .EndComposite();

            _tree = builder.Build();
        }

        public void Update()
        {
            _attack = false;
            _tree.Tick();
        }

        TaskStatus FindTarget()
        {
            _moving = true;
            if (_collision.Collision.Right || _collision.Collision.Left)
                _dir = -_dir;
            if (Entity.Position.X <= 0)
                _dir = 1f;

            var hit = Physics.Linecast(
                Entity.Position,
                Entity.Position + new Vector2(LinecastDistance * _dir, 0),
                _enemyMask);
            if (hit.Collider != null)
            {
                _attack = true;
                _moving = false;
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}
