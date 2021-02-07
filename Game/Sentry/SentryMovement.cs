using Game.Editor.Prefab;
using Game.Platformer;
using Nez;
using Nez.AI.FSM;
using static Game.Editor.Prefab.PlayerMovementData;

namespace Game.Sentry
{
    class SentryMovementData : DataComponent
    {
        public AnimationData IdleRight = new AnimationData();
        public AnimationData IdleLeft = new AnimationData();
        public AnimationData WalkRight = new AnimationData();
        public AnimationData WalkLeft = new AnimationData();

        public AnimationData TurtleRight = new AnimationData();
        public AnimationData TurtleLeft = new AnimationData();
        public AnimationData UnturtleRight = new AnimationData();
        public AnimationData UnturtleLeft = new AnimationData();

        public float MoveSpeed = 100f;

        public override void AddToEntity(Entity entity)
        {
            var movement = entity.AddComponent<PlatformerMovement>();
            movement.MoveSpeed = MoveSpeed;
            movement.MakeFSM = MakeFSM;

            entity.AddComponent<Animator<ObserverFrame>>();
            entity.AddComponent<SpriteObserver>();
            entity.AddComponent<PlatformerMover>();
        }

        StateMachine<MovementContext> MakeFSM(MovementContext ctx)
        {
            var groundState = new GroundState();
            groundState.Idle.Right = IdleRight.MakeAnimation();
            groundState.Idle.Left = IdleLeft.MakeAnimation();
            groundState.Walk.Right = WalkRight.MakeAnimation();
            groundState.Walk.Left = WalkLeft.MakeAnimation();
            var fsm = new StateMachine<MovementContext>(ctx, groundState);

            var turtleState = new TurtleState();
            turtleState.Turtle.Right = TurtleRight.MakeAnimation();
            turtleState.Turtle.Left = TurtleLeft.MakeAnimation();
            turtleState.Unturtle.Right = UnturtleRight.MakeAnimation();
            turtleState.Unturtle.Left = UnturtleLeft.MakeAnimation();
            fsm.AddState(turtleState);

            return fsm;
        }
    }
}
