using Microsoft.Xna.Framework;
using Nez.AI.FSM;
using System;

namespace Game.Movement
{
    partial class PlayerMovement
    {
        class GroundState : State<PlayerMovement>
        {
            public override void Begin()
            {
                _context._velocity.Y = 0;
            }

            public override void Reason()
            {
                if (!_context._collisionBelow)
                {
                    _machine.ChangeState<AirState>();
                }
            }

            public override void Update(float deltaTime)
            {
                _context._velocity.Y += _context.Gravity * deltaTime; // ensure collision below

                var moveDir = new Vector2(_context._controller.XAxis, 0);
                _context._velocity.X = _context.MoveSpeed * moveDir.X;
                if (moveDir.X != 0)
                    _context._facing = Math.Sign(moveDir.X);

                if (_context._controller.JumpPressed)
                {
                    _context._velocity.Y = -_context.JumpVelocity;
                }

                _context.Move(deltaTime);

                if (_context._collisionBelow)
                {
                    _context._velocity.Y = 0;
                }

                if (moveDir.X != 0)
                    _context.ChangeAnimation(_context._facing > 0 ? "WalkRight" : "WalkLeft");
                else
                    _context.ChangeAnimation(_context._facing > 0 ? "IdleRight" : "IdleLeft");
            }
        }
    }
}
