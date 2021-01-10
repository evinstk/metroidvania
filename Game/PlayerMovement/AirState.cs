using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;

namespace Game.Movement
{
    partial class PlayerMovement
    {
        class AirState : State<PlayerMovement>
        {
            public override void Reason()
            {
                if (_context._collisionBelow)
                {
                    _machine.ChangeState<GroundState>();
                }
            }

            public override void Update(float deltaTime)
            {
                _context._jumpElapsed += deltaTime;

                // y-dir
                if ((_context._velocity.Y < 0 && _context._controller.JumpReleased) || _context._collisionAbove)
                {
                    _context._velocity.Y = 0;
                }
                if (_context._velocity.Y >= 0 || !_context._controller.JumpDown || _context._jumpElapsed >= _context.JumpDuration)
                {
                    _context._velocity.Y += _context.Gravity * deltaTime;
                }
                _context._velocity.Y = Mathf.Clamp(_context._velocity.Y, -_context.JumpVelocity, _context.MaxFallVelocity);

                // x-dir
                var moveDir = new Vector2(_context._controller.XAxis, 0);
                _context._velocity.X = _context.MoveSpeed * moveDir.X;

                _context.Move(deltaTime);
                _context.ChangeAnimation(_context.Jump);
            }
        }
    }
}
