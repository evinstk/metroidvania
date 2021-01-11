using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;

namespace Game.Movement
{
    struct MovementAnimation
    {
        public Animation<ObserverFrame> Left;
        public Animation<ObserverFrame> Right;
    }

    partial class PlayerMovement :
#if DEBUG
        RenderableComponent,
#else
        Component,
#endif
        IUpdatable
    {
        public float Gravity = 750; // acceleration
        public float MoveSpeed = 150;
        public float MaxFallVelocity = 200;
        public float JumpVelocity = 250;
        public float JumpDuration = 0.2f;
        public int HitMask = -1;

        int _facing = 1;

        // animations
        public MovementAnimation Idle;
        public MovementAnimation Walk;
        public MovementAnimation Attack;
        public MovementAnimation Jump;

        // external dependencies
        ControllerComponent _controller;

        Microsoft.Xna.Framework.Vector2 _velocity;
        SubpixelFloat _subpixelX;
        SubpixelFloat _subpixelY;
        bool _collisionBelow = true;
        bool _collisionAbove;
        float _jumpElapsed;

        Mover _mover;
        Animator<ObserverFrame> _animator;
        SpriteRenderer _renderer;
        StateMachine<PlayerMovement> _fsm;

        public override void OnAddedToEntity()
        {
            _controller = Entity.GetComponent<ControllerComponent>();
            if (_controller == null)
                _controller = Entity.AddComponent<FreeController>();

            _mover = Entity.GetComponentStrict<Mover>();
            _fsm = new StateMachine<PlayerMovement>(this, new GroundState());
            _fsm.AddState(new AirState());
            _fsm.AddState(new AttackState());

            _renderer = Entity.GetComponentStrict<SpriteRenderer>();

            _animator = Entity.GetComponentStrict<Animator<ObserverFrame>>();
            _animator.OnFrameEnter += HandleFrameEnter;
            _animator.Play(Idle.Right);

            Entity.GetOrCreateComponent<BoxCollider>();
        }

        void HandleFrameEnter(ObserverFrame frame)
        {
            _renderer.SetSprite(frame.Sprite);
            _renderer.FlipX = frame.Flip;
            for (var i = 0; i < frame.HitBoxes.Length; ++i)
            {
                CastHitBox(frame.HitBoxes[i], frame.Flip);
            }
#if DEBUG
            _flip = frame.Flip;
            _hitboxes = frame.HitBoxes;
#endif
        }

        Collider[] _colliderResults = new Collider[8];
        void CastHitBox(RectangleF hitbox, bool flip)
        {
            var location = hitbox.Location;
            if (flip) location.X = -location.X - hitbox.Width;
            hitbox.Location = location + Entity.Position;
            Physics.OverlapRectangleAll(ref hitbox, _colliderResults, HitMask);
            for (var i = 0; i < _colliderResults.Length; ++i)
            {
                var collider = _colliderResults[i];
                if (collider != null)
                {
                    // handle attack collision
                    collider.Entity.Destroy();
                }
                _colliderResults[i] = null;
            }
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
        }

        void Move(float deltaTime)
        {
            var motion = _velocity * deltaTime;
            _subpixelX.Update(ref motion.X);
            _subpixelY.Update(ref motion.Y);
            if (_collisionBelow && motion.Y == 0 && _subpixelY.Remainder > 0)
            {
                motion.Y = 1;
                _subpixelY.Reset();
            }
            _mover.CalculateMovement(ref motion, out var collisionResult);

            _collisionBelow = collisionResult.Normal.Y < 0;
            _collisionAbove = collisionResult.Normal.Y > 0;

            if (collisionResult.Normal.X != 0) _subpixelX.Reset();
            if (collisionResult.Normal.Y != 0) _subpixelY.Reset();

            _mover.ApplyMovement(motion);
        }

        void ChangeAnimation(
            MovementAnimation animationPack,
            Animator<ObserverFrame>.LoopMode loopMode = Animator<ObserverFrame>.LoopMode.Loop)
        {
            var animation = _facing > 0 ? animationPack.Right : animationPack.Left;
            if (!_animator.IsAnimationActive(animation))
                _animator.Play(animation, loopMode);
        }

#if DEBUG
        RectangleF[] _hitboxes = null;
        bool _flip = false;

        public override RectangleF Bounds => _renderer.Bounds;

        public override void Render(Batcher batcher, Camera camera)
        {
        }

        public override void DebugRender(Batcher batcher)
        {
            if (_hitboxes != null)
            {
                var color = Color.DarkRed;
                color.A = 20;
                for (var i = 0; i < _hitboxes.Length; ++i)
                {
                    var hitbox = _hitboxes[i];
                    var location = hitbox.Location;
                    if (_flip) location.X = -location.X - hitbox.Width;
                    hitbox.Location = location + Entity.Position;
                    batcher.DrawRect(hitbox, color);
                }
            }
        }
#endif
    }
}
