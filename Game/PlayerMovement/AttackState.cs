using Nez.AI.FSM;

namespace Game.Movement
{
    partial class PlayerMovement
    {
        class AttackState : State<PlayerMovement>
        {
            public override void Begin()
            {
                _context.ChangeAnimation(_context.Attack, Animator<ObserverFrame>.LoopMode.ClampForever);
            }

            public override void Reason()
            {
                if (!_context._animator.IsRunning)
                {
                    if (_context._collisionBelow)
                        _machine.ChangeState<GroundState>();
                    else
                        _machine.ChangeState<AirState>();
                }
            }

            public override void Update(float deltaTime)
            {
            }
        }
    }
}
