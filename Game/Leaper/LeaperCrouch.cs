using Nez.AI.FSM;

namespace Game.Leaper
{
    partial class LeaperMovement
    {
        class LeaperCrouch : State<LeaperMovement>
        {
            public override void Begin()
            {
                _context.ChangeAnimation(_context.Crouch, Animator<ObserverFrame>.LoopMode.ClampForever);
            }

            public override void Reason()
            {
                if (!_context._animator.IsRunning)
                    _machine.ChangeState<LeaperLeap>();
            }

            public override void Update(float deltaTime)
            {
            }
        }
    }
}
