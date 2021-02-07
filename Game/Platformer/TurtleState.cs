using Game.Movement;
using Nez.AI.FSM;

namespace Game.Platformer
{
    class TurtleState : State<MovementContext>
    {
        public bool ToGround = false;

        public MovementAnimation Turtle;
        public MovementAnimation Unturtle;

        public override void Begin()
        {
            _context.ChangeAnimation(Turtle, Animator<ObserverFrame>.LoopMode.ClampForever);
        }

        public override void Reason()
        {
            if (!_context.Animator.IsRunning && _context.GetIsCurrentAnimation(Unturtle))
                _machine.ChangeState<GroundState>();
        }

        public override void Update(float deltaTime)
        {
            if (!_context.Animator.IsRunning
                && ToGround
                && _context.GetIsCurrentAnimation(Turtle))
            {
                _context.ChangeAnimation(Unturtle, Animator<ObserverFrame>.LoopMode.ClampForever);
            }
            ToGround = false;
        }
    }
}
