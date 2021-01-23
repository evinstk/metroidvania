using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using static Game.PlatformerMover;

namespace Game.Movement
{
    struct MovementAnimation
    {
        public Animation<ObserverFrame> Left;
        public Animation<ObserverFrame> Right;
    }

    partial class PlayerMovement : Component, IUpdatable
    {
        public float Gravity = 750; // acceleration
        public float MoveSpeed = 150;
        public float MaxFallVelocity = 200;
        public float JumpVelocity = 250;
        public float JumpDuration = 0.2f;

        int _facing = 1;

        // animations
        public MovementAnimation Idle;
        public MovementAnimation Walk;
        public MovementAnimation Attack;
        public MovementAnimation Jump;
        public MovementAnimation Talking;

        // external dependencies
        ControllerComponent _controller;

        Microsoft.Xna.Framework.Vector2 _velocity;
        float _jumpElapsed;
        bool _talkPending;
        bool _restPending;

        PlatformerMover _platformerMover;
        BoxCollider _boxCollider;
        CollisionState _collisionState = new CollisionState
        {
            Below = true
        };

        Animator<ObserverFrame> _animator;
        StateMachine<PlayerMovement> _fsm;

        public void Talk() => _talkPending = true;
        public void Rest() => _restPending = true;

        public override void OnAddedToEntity()
        {
            _controller = Entity.GetComponent<ControllerComponent>();
            if (_controller == null)
            {
                Debug.Log($"No {typeof(ControllerComponent).Name} on {Entity.Name}. Adding {typeof(FreeController).Name}.");
                _controller = Entity.AddComponent<FreeController>();
            }

            _platformerMover = Entity.GetComponentStrict<PlatformerMover>();
            _boxCollider = Entity.GetComponentStrict<BoxCollider>();

            _fsm = new StateMachine<PlayerMovement>(this, new GroundState());
            _fsm.AddState(new AirState());
            _fsm.AddState(new AttackState());
            _fsm.AddState(new TalkingState());

            _animator = Entity.GetComponentStrict<Animator<ObserverFrame>>();
            _animator.Play(Idle.Right);

            Entity.GetOrCreateComponent<BoxCollider>();
        }

        public void Update()
        {
            Reason();
            _fsm.Update(Time.DeltaTime);
        }

        void Reason()
        {
            if (_controller.AttackPressed)
            {
                _fsm.ChangeState<AttackState>();
            }
            if (_talkPending && _fsm.CurrentState != _fsm.GetState<TalkingState>())
            {
                _talkPending = false;
                _fsm.ChangeState<TalkingState>();
            }
            if (_restPending && _fsm.CurrentState == _fsm.GetState<TalkingState>())
            {
                _restPending = false;
                _fsm.ChangeState<GroundState>();
            }
        }

        void Move(float deltaTime)
        {
            _velocity.Y = Mathf.Clamp(_velocity.Y, -JumpVelocity, MaxFallVelocity);
            var motion = _velocity * deltaTime;
            _platformerMover.Move(motion, _boxCollider, _collisionState);
            if (_collisionState.Below)
            {
                _velocity.Y = 0;
            }
        }

        void ChangeAnimation(
            MovementAnimation animationPack,
            Animator<ObserverFrame>.LoopMode loopMode = Animator<ObserverFrame>.LoopMode.Loop)
        {
            var animation = _facing > 0 ? animationPack.Right : animationPack.Left;
            if (!_animator.IsAnimationActive(animation))
                _animator.Play(animation, loopMode);
        }
    }
}
