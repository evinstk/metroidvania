using Microsoft.Xna.Framework;
using Nez.AI.FSM;
using System;

namespace Game.Leaper
{
    partial class LeaperMovement
    {
        class LeaperGround : State<LeaperMovement>
        {
            public override void Reason()
            {
                if (_context._controller.Leap)
                    _machine.ChangeState<LeaperCrouch>();
            }

            public override void Update(float deltaTime)
            {
                if (_context._collisionState.Below)
                    _context._velocity.Y = 1;
                else
                    _context._velocity.Y += deltaTime * _context.Gravity;

                var motionX = _context._controller.XAxis;
                _context._velocity.X = motionX * _context.Speed;
                if (motionX != 0)
                    _context._facing = Math.Sign(motionX);

                _context.Move();

                if (motionX != 0)
                    _context.ChangeAnimation(_context.Walk);
                else
                    _context.ChangeAnimation(_context.Idle);
            }
        }
    }
}
