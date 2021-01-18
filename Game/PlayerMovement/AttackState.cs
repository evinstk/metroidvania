using Microsoft.Xna.Framework;
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
                    if (_context._collisionState.Below)
                        _machine.ChangeState<GroundState>();
                    else
                        _machine.ChangeState<AirState>();
                }
            }

            public override void Update(float deltaTime)
            {
                _context._jumpElapsed += deltaTime;

                if ((_context._velocity.Y < 0 && _context._controller.JumpReleased) || _context._collisionState.Above)
                {
                    _context._velocity.Y = 0;
                }
                if (_context._velocity.Y >= 0 || !_context._controller.JumpDown || _context._jumpElapsed >= _context.JumpDuration)
                {
                    _context._velocity.Y += _context.Gravity * deltaTime;
                }

                var moveDir = new Vector2(_context._controller.XAxis, 0);
                _context._velocity.X = _context.MoveSpeed * moveDir.X;

                _context.Move(deltaTime);
            }
        }
    }
}
