using Game.Editor.Prefab;
using Game.Hit;
using Game.Movement;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using System.Collections.Generic;
using static Game.Editor.Prefab.PlayerMovementData;
using static Game.PlatformerMover;

namespace Game.Leaper
{
    class LeaperMovementData : DataComponent
    {
        public float Speed = 100f;
        public float Gravity = 750f;
        public float MaxFallVelocity = 200;
        public float JumpVelocity = 150;
        public float JumpDuration = 0.2f;
        public int SwipeHeight = 48;
        public HitData HitMask = new HitData();

        public AnimationData IdleRight = new AnimationData();
        public AnimationData IdleLeft = new AnimationData();
        public AnimationData WalkRight = new AnimationData();
        public AnimationData WalkLeft = new AnimationData();
        public AnimationData CrouchRight = new AnimationData();
        public AnimationData CrouchLeft = new AnimationData();
        public AnimationData LeapRight = new AnimationData();
        public AnimationData LeapLeft = new AnimationData();
        public AnimationData SwipeRight = new AnimationData();
        public AnimationData SwipeLeft = new AnimationData();

        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<PlatformerMover>();
            entity.AddComponent<SpriteObserver>();
            entity.AddComponent<Animator<ObserverFrame>>();
            var movement = entity.AddComponent<LeaperMovement>();

            movement.Speed = Speed;
            movement.Gravity = Gravity;
            movement.MaxFallVelocity = MaxFallVelocity;
            movement.JumpVelocity = JumpVelocity;
            movement.JumpDuration = JumpDuration;
            movement.SwipeHeight = SwipeHeight;

            movement.Idle.Right = IdleRight.MakeAnimation();
            movement.Idle.Left = IdleLeft.MakeAnimation();
            movement.Walk.Right = WalkRight.MakeAnimation();
            movement.Walk.Left = WalkLeft.MakeAnimation();
            movement.Crouch.Right = CrouchRight.MakeAnimation();
            movement.Crouch.Left = CrouchLeft.MakeAnimation();
            movement.Leap.Right = LeapRight.MakeAnimation();
            movement.Leap.Left = LeapLeft.MakeAnimation();
            movement.Swipe.Right = SwipeRight.MakeAnimation();
            movement.Swipe.Left = SwipeLeft.MakeAnimation();

            var hitter = entity.AddComponent<Hitter>();
            hitter.HitMask = HitMask.HitMask;
        }
    }

    interface ILeaperController
    {
        float XAxis { get; }
        bool Leap { get; }
    }

    partial class LeaperMovement : Component, IUpdatable
    {
        public float Speed = 100f;
        public float Gravity = 750f;
        public float MaxFallVelocity = 200;
        public float JumpVelocity = 150;
        public float JumpDuration = 0.2f;
        public int SwipeHeight = 48;

        public MovementAnimation Idle;
        public MovementAnimation Walk;
        public MovementAnimation Crouch;
        public MovementAnimation Leap;
        public MovementAnimation Swipe;

        Microsoft.Xna.Framework.Vector2 _velocity;
        CollisionState _collisionState = new CollisionState { Below = true };
        int _facing = 1;
        float _jumpElapsed = 0;
        StateMachine<LeaperMovement> _fsm;

        ILeaperController _controller;
        BoxCollider _collider;
        PlatformerMover _mover;
        Animator<ObserverFrame> _animator;

        public override void OnAddedToEntity()
        {
            var controller = new List<ILeaperController>();
            Entity.GetComponents(controller);
            if (controller.Count > 0)
            {
                _controller = controller[0];
            }
            else
            {
                _controller = Entity.AddComponent<FreeLeaperController>();
                Debug.Log($"No {typeof(ILeaperController).Name} on {Entity.Name}. Supplying {typeof(FreeLeaperController).Name}.");
            }

            _collider = Entity.GetComponent<BoxCollider>();
            if (_collider == null)
            {
                _collider = Entity.AddComponent<BoxCollider>();
                Debug.Log($"No {typeof(BoxCollider).Name} on {Entity.Name}. Supplying {typeof(BoxCollider).Name}.");
            }

            if (!Entity.HasComponent<SpriteRenderer>())
            {
                Entity.AddComponent<SpriteRenderer>();
                Debug.Log($"No {typeof(SpriteRenderer).Name} on {Entity.Name}. Supplying {typeof(SpriteRenderer).Name}.");
            }

            _mover = Entity.GetComponentStrict<PlatformerMover>();

            _animator = Entity.GetComponentStrict<Animator<ObserverFrame>>();
            _animator.Play(Idle.Right);

            _fsm = new StateMachine<LeaperMovement>(this, new LeaperGround());
            _fsm.AddState(new LeaperLeap());
            _fsm.AddState(new LeaperCrouch());
        }

        public void Update()
        {
            var start = Time.TimeSinceSceneLoad;
            var dt = Time.DeltaTime;
            if (start - dt > 0 && dt > 0)
                _fsm.Update(Time.DeltaTime);
        }

        void Move()
        {
            _velocity.Y = Mathf.Clamp(_velocity.Y, -JumpVelocity, MaxFallVelocity);
            var motion = _velocity * Time.DeltaTime;
            _mover.Move(motion, _collider, _collisionState);
            if (_collisionState.Below)
                _velocity.Y = 0;
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
