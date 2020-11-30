using Game.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using System.IO;
using System.Linq;

namespace Game
{
    class AnimatorContext
    {
        public CollisionComponent Collision;
        public Animator<Frame> Animator;
        public MobMover Mover;

        public void PlayAnimation(string animation, Animator<Frame>.LoopMode? loopMode = null)
        {
            Animator.Play(animation, loopMode);
        }
    }

    class FrameOptions
    {
        public bool FlipX = false;
        public TiledObject HitBoxData;
        public SoundEffect Sound;
    }

    class Frame : IFrame
    {
        SpriteRenderer _renderer;
        HitBoxComponent _hitC;

        Sprite _sprite;
        FrameOptions _options;

        public Frame(
            SpriteRenderer renderer,
            HitBoxComponent hitC,
            Sprite sprite,
            FrameOptions options)
        {
            _renderer = renderer;
            _hitC = hitC;
            _sprite = sprite;
            _options = options;
        }

        public void OnEnter()
        {
            _renderer.Sprite = _sprite;
            _renderer.FlipX = _options.FlipX;

            var hitBoxData = _options.HitBoxData;
            _hitC.SetEnabled(hitBoxData != null);
            if (hitBoxData != null)
            {
                _hitC.Collider.SetSize(hitBoxData.Width, hitBoxData.Height);
                _hitC.Collider.SetLocalOffset(new Vector2(
                    (hitBoxData.X + .5f * (hitBoxData.Width - _renderer.Bounds.Width)) * (_options.FlipX ? -1 : 1),
                    hitBoxData.Y + .5f * (hitBoxData.Height - _renderer.Bounds.Height)));
            }

            var sound = _options.Sound;
            if (sound != null)
            {
                sound.Play();
            }
        }
    }

    class AnimationMachine : Component, IUpdatable
    {
        string _animationGroup;
        StateMachine<AnimatorContext> _fsm;

        public AnimationMachine(string animationGroup)
        {
            _animationGroup = animationGroup;
        }

        public override void OnAddedToEntity()
        {
            var collision = Entity.GetComponent<CollisionComponent>();
            Insist.IsNotNull(collision);
            var mover = Entity.GetComponent<MobMover>();
            Insist.IsNotNull(mover);
            var spriteRenderer = Entity.GetComponent<SpriteRenderer>();
            Insist.IsNotNull(spriteRenderer);
            var hitC = Entity.GetComponent<HitBoxComponent>();
            Insist.IsNotNull(hitC);

            var animator = Entity.AddAnimator(_animationGroup);

            _fsm = new StateMachine<AnimatorContext>(new AnimatorContext
            {
                Collision = collision,
                Animator = animator,
                Mover = mover,
            }, new IdleState());
            _fsm.AddState(new WalkState());
            _fsm.AddState(new AttackState());
            _fsm.AddState(new JumpState());
            _fsm.AddState(new LandState());
        }

        public void Update()
        {
            _fsm.Update(Time.DeltaTime);
        }
    }

    class IdleState : State<AnimatorContext>
    {
        public override void Begin()
        {
            _context.PlayAnimation(_context.Mover.Facing > 0
                ? "IdleRight" : "IdleLeft");
        }

        public override void Update(float deltaTime)
        {
            if (_context.Mover.Velocity.Y < 0)
            {
                _machine.ChangeState<JumpState>();
            }
            if (_context.Mover.Velocity.X != 0)
            {
                _machine.ChangeState<WalkState>();
            }
            if (_context.Mover.AttackInput)
            {
                _machine.ChangeState<AttackState>();
            }
        }
    }

    class WalkState : State<AnimatorContext>
    {
        public override void Begin()
        {
            _context.PlayAnimation(_context.Mover.Facing > 0
                ? "WalkRight" : "WalkLeft");
        }

        public override void Update(float deltaTime)
        {
            if (_context.Mover.Facing > 0 && _context.Animator.IsAnimationActive("WalkLeft"))
            {
                _context.PlayAnimation("WalkRight");
            }
            if (_context.Mover.Facing < 0 && _context.Animator.IsAnimationActive("WalkRight"))
            {
                _context.PlayAnimation("WalkLeft");
            }
            if (_context.Mover.Velocity.Y < 0)
            {
                _machine.ChangeState<JumpState>();
            }
            if (_context.Mover.Velocity.X == 0 && _context.Collision.Collision.Below)
            {
                _machine.ChangeState<IdleState>();
            }
            if (_context.Mover.AttackInput)
            {
                _machine.ChangeState<AttackState>();
            }
        }
    }

    class AttackState : State<AnimatorContext>
    {
        public override void Begin()
        {
            _context.PlayAnimation(_context.Mover.Facing > 0
                ? "AttackRight" : "AttackLeft", Animator<Frame>.LoopMode.Once);
            _context.Animator.OnAnimationCompletedEvent += HandleComplete;
        }

        void HandleComplete(string animation)
        {
            _machine.ChangeState<IdleState>();
        }

        public override void End()
        {
            _context.Animator.OnAnimationCompletedEvent -= HandleComplete;
        }

        public override void Update(float deltaTime)
        {
        }
    }

    class JumpState : State<AnimatorContext>
    {
        public override void Begin()
        {
            _context.PlayAnimation(_context.Mover.Facing > 0
                ? "JumpRight" : "JumpLeft", Animator<Frame>.LoopMode.ClampForever);
        }

        public override void Update(float deltaTime)
        {
            if (_context.Mover.Velocity.Y == 0)
            {
                _machine.ChangeState<LandState>();
            }
            if (_context.Mover.AttackInput)
            {
                _machine.ChangeState<AttackState>();
            }
        }
    }

    class LandState : State<AnimatorContext>
    {
        public override void Begin()
        {
            _context.PlayAnimation(_context.Mover.Facing > 0
                ? "LandRight" : "LandLeft", Animator<Frame>.LoopMode.ClampForever);
            _context.Animator.OnAnimationCompletedEvent += HandleComplete;
        }

        void HandleComplete(string animation)
        {
            _machine.ChangeState<IdleState>();
        }

        public override void End()
        {
            _context.Animator.OnAnimationCompletedEvent -= HandleComplete;
        }

        public override void Update(float deltaTime)
        {
        }
    }
}
