using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;

namespace Game.Movement
{
    partial class PlayerMovement : Component, IUpdatable
    {
        public float Gravity = 300; // acceleration
        public float MoveSpeed = 130;
        public float MaxFallVelocity = 150;
        public float JumpVelocity = 200;
        public float JumpDuration = 0.2f;

        // external dependencies
        ControllerComponent _controller;

        Vector2 _velocity;
        SubpixelFloat _subpixelX;
        SubpixelFloat _subpixelY;
        bool _collisionBelow;
        bool _collisionAbove;
        float _jumpElapsed;

        Mover _mover;
        StateMachine<PlayerMovement> _fsm;

        public override void OnAddedToEntity()
        {
            _controller = Entity.GetComponent<ControllerComponent>();
            Insist.IsNotNull(_controller);

            _mover = Entity.AddComponent<Mover>();
            _fsm = new StateMachine<PlayerMovement>(this, new GroundState());
            _fsm.AddState(new AirState());
        }

        public void Update()
        {
            _fsm.Update(Time.DeltaTime);
        }

        void Move(float deltaTime)
        {
            var motion = _velocity * deltaTime;
            _mover.CalculateMovement(ref motion, out var collisionResult);
            _subpixelX.Update(ref motion.X);
            _subpixelY.Update(ref motion.Y);

            _collisionBelow = collisionResult.Normal.Y < 0;
            _collisionAbove = collisionResult.Normal.Y > 0;

            if (collisionResult.Normal.X != 0) _subpixelX.Reset();
            if (collisionResult.Normal.Y != 0) _subpixelY.Reset();

            _mover.ApplyMovement(motion);
        }
    }
}
