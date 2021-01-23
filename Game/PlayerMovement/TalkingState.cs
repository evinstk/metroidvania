using Nez;
using Nez.AI.FSM;

namespace Game.Movement
{
    partial class PlayerMovement
    {
        class TalkingState : State<PlayerMovement>
        {
            public override void Update(float deltaTime)
            {
                _context.ChangeAnimation(_context.Talking);
            }
        }
    }
}
