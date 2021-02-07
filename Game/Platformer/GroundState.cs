using System;
using Game.Movement;
using Nez.AI.FSM;

namespace Game.Platformer
{
    class GroundState : State<MovementContext>
    {
        public float MotionX;
        public bool ToTurtle;

        public MovementAnimation Idle;
        public MovementAnimation Walk;

        public override void Reason()
        {
            if (ToTurtle)
                _machine.ChangeState<TurtleState>();
            ToTurtle = false;
        }

        public override void Update(float deltaTime)
        {
            _context.Velocity.Y = 1;

            _context.Velocity.X = _context.Movement.MoveSpeed * MotionX;
            if (MotionX != 0)
                _context.Facing = Math.Sign(MotionX);

            _context.Move(deltaTime);

            if (MotionX != 0)
                _context.ChangeAnimation(Walk);
            else
                _context.ChangeAnimation(Idle);

            MotionX = 0;
        }
    }

}
