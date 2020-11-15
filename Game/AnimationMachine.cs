using Game.Tiled;
using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using System.IO;
using System.Linq;
using static Game.Animator;

namespace Game
{
    class AnimatorContext
    {
        public CollisionComponent Collision;
        public Animator Animator;
        public MobMover Mover;

        public void PlayAnimation(string animation, LoopMode? loopMode = null)
        {
            Animator.Play(animation, loopMode);
        }
    }

    class AnimationMachine : Component, IUpdatable
    {
        string _animationGroup;
        Color _color;
        StateMachine<AnimatorContext> _fsm;

        public AnimationMachine(string animationGroup, Color? color = null)
        {
            _animationGroup = animationGroup;
            _color = color ?? Color.White;
        }

        public override void OnAddedToEntity()
        {
            var group = Entity.Scene.Content.Load<Data.AnimationGroup[]>("Data/AnimationGroups").First(g => g.Name == _animationGroup);

            var animator = Entity.AddComponent<Animator>();

            var collision = Entity.GetComponent<CollisionComponent>();
            Insist.IsNotNull(collision);
            var mover = Entity.GetComponent<MobMover>();
            Insist.IsNotNull(mover);
            var spriteRenderer = Entity.GetComponent<SpriteRenderer>();
            Insist.IsNotNull(spriteRenderer);

            foreach (var animType in group.AnimationTypes)
            {
                var tileset = Tileset.Load("Content/Animations/" + animType.Tileset + ".json");
                var anim = tileset.Tiles.First(t => t.Type == animType.Name);
                var texture = Entity.Scene.Content.LoadTexture("Textures/" + Path.GetFileNameWithoutExtension(tileset.Image));
                var sprites = Sprite.SpritesFromAtlas(texture, tileset.TileWidth, tileset.TileHeight);
                var frames = anim.Animation.Select(f => new Frame(
                    sprites[f.TileId],
                    spriteRenderer,
                    new FrameOptions
                    {
                        FlipX = animType.Flip,
                        Color = _color,
                    })).ToArray();
                animator.AddAnimation(
                    animType.Type,
                    new Animation(
                        frames,
                        12));
            }

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
                ? "AttackRight" : "AttackLeft", LoopMode.Once);
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
                ? "JumpRight" : "JumpLeft", LoopMode.ClampForever);
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
                ? "LandRight" : "LandLeft", LoopMode.ClampForever);
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
