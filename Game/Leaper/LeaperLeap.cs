using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using System;

namespace Game.Leaper
{
    partial class LeaperMovement
    {
        class LeaperLeap : State<LeaperMovement>
        {
            public override void Begin()
            {
                _context._velocity.Y = -_context.JumpVelocity;
                _context._jumpElapsed = 0;
                _context._facing = Math.Sign(_context._controller.XAxis);

                _context.ChangeAnimation(_context.Leap);
            }

            public override void Reason()
            {
                if (_context._collisionState.Below)
                    _machine.ChangeState<LeaperGround>();
            }

            public override void Update(float deltaTime)
            {
                _context._jumpElapsed += deltaTime;

                if (_context._collisionState.Above)
                    _context._velocity.Y = 0;
                if (_context._jumpElapsed > _context.JumpDuration)
                    _context._velocity.Y += _context.Gravity * deltaTime;

                _context._velocity.X = _context._facing * _context.Speed;
                _context.Move();

                if (_context._velocity.Y > 0)
                {
                    var position = _context.Entity.Position;
                    var mask = 0;
                    Flags.SetFlagExclusive(ref mask, PhysicsLayer.Terrain);
                    var hit = Physics.Linecast(position, position + new Vector2(0, _context.SwipeHeight), mask);
                    if (hit.Collider != null)
                        _context.ChangeAnimation(_context.Swipe, Animator<ObserverFrame>.LoopMode.ClampForever);
                }
            }
        }
    }
}
