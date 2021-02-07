using Game.Movement;
using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using System;
using static Game.PlatformerMover;

namespace Game.Platformer
{
    class PlatformerMovement : Component, IUpdatable
    {
        public float MoveSpeed;

        public Func<MovementContext, StateMachine<MovementContext>> MakeFSM;

        public int Facing => _ctx.Facing;

        StateMachine<MovementContext> _fsm;
        public float ElapsedTimeInState => _fsm.ElapsedTimeInState;
        MovementContext _ctx;

        public override void OnAddedToEntity()
        {
            _ctx = new MovementContext
            {
                Entity = Entity,
                Movement = this,
                Mover = Entity.GetComponentStrict<PlatformerMover>(),
                Collider = Entity.GetOrCreateComponent<BoxCollider>(),
                Animator = Entity.GetComponentStrict<Animator<ObserverFrame>>(),
            };
            Insist.IsNotNull(MakeFSM);
            _fsm = MakeFSM.Invoke(_ctx);
        }

        public void Update()
        {
            var dt = Time.DeltaTime;
            if (dt > 0)
            {
                _fsm.Update(dt);
            }
        }

        public State<MovementContext> CurrentState => _fsm.CurrentState;
    }

    class MovementContext
    {
        public Entity Entity;
        public PlatformerMovement Movement;

        public Vector2 Velocity;
        public PlatformerMover Mover;
        public BoxCollider Collider;
        public CollisionState CollisionState = new CollisionState
        {
            Below = true,
        };
        public int Facing = 1;
        public Animator<ObserverFrame> Animator;

        public void Move(float deltaTime)
        {
            var motion = Velocity * deltaTime;
            Mover.Move(motion, Collider, CollisionState);
        }

        public void ChangeAnimation(
            MovementAnimation animationPack,
            Animator<ObserverFrame>.LoopMode loopMode = Animator<ObserverFrame>.LoopMode.Loop)
        {
            var animation = Facing > 0 ? animationPack.Right : animationPack.Left;
            if (!Animator.IsAnimationActive(animation))
                Animator.Play(animation, loopMode);
        }

        public bool GetIsCurrentAnimation(MovementAnimation animationPack)
        {
            var currentAnimation = Animator.CurrentAnimation;
            return currentAnimation == animationPack.Right || currentAnimation == animationPack.Left;
        }
    }
}
