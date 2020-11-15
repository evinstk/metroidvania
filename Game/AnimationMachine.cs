using Game.Tiled;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static Nez.Sprites.SpriteAnimator;

namespace Game
{
    class AnimatorContext
    {
        public CollisionComponent Collision;
        public SpriteAnimator SpriteAnimator;
        public MobMover Mover;
        public Dictionary<string, AnimationMeta> Meta;

        public void PlayAnimation(string animation, LoopMode? loopMode = null)
        {
            SpriteAnimator.Play(animation, loopMode);
            var meta = Meta[animation];
            SpriteAnimator.FlipX = meta.Flip;
        }
    }

    class AnimationMeta
    {
        public bool Flip;
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

            var spriteAnimator = Entity.AddComponent<SpriteAnimator>();
            spriteAnimator.Color = _color;

            var collision = Entity.GetComponent<CollisionComponent>();
            Insist.IsNotNull(collision);
            var mover = Entity.GetComponent<MobMover>();
            Insist.IsNotNull(mover);

            foreach (var animType in group.AnimationTypes)
            {
                var tileset = Tileset.Load("Content/Animations/" + animType.Tileset + ".json");
                var anim = tileset.Tiles.First(t => t.Type == animType.Name);
                var texture = Entity.Scene.Content.LoadTexture("Textures/" + Path.GetFileNameWithoutExtension(tileset.Image));
                var sprites = Sprite.SpritesFromAtlas(texture, tileset.TileWidth, tileset.TileHeight);
                var frames = anim.Animation.Select(f => sprites[f.TileId]).ToArray();
                spriteAnimator.AddAnimation(
                    animType.Type,
                    frames,
                    12);
            }

            var meta = group.AnimationTypes.ToDictionary(t => t.Type, t =>
            {
                return new AnimationMeta
                {
                    Flip = t.Flip,
                };
            });

            _fsm = new StateMachine<AnimatorContext>(new AnimatorContext
            {
                Collision = collision,
                SpriteAnimator = spriteAnimator,
                Mover = mover,
                Meta = meta,
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
            if (_context.Mover.Facing > 0 && _context.SpriteAnimator.IsAnimationActive("WalkLeft"))
            {
                _context.PlayAnimation("WalkRight");
            }
            if (_context.Mover.Facing < 0 && _context.SpriteAnimator.IsAnimationActive("WalkRight"))
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
            _context.SpriteAnimator.OnAnimationCompletedEvent += HandleComplete;
        }

        void HandleComplete(string animation)
        {
            _machine.ChangeState<IdleState>();
        }

        public override void End()
        {
            _context.SpriteAnimator.OnAnimationCompletedEvent -= HandleComplete;
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
            _context.SpriteAnimator.OnAnimationCompletedEvent += HandleComplete;
        }

        void HandleComplete(string animation)
        {
            _machine.ChangeState<IdleState>();
        }

        public override void End()
        {
            _context.SpriteAnimator.OnAnimationCompletedEvent -= HandleComplete;
        }

        public override void Update(float deltaTime)
        {
        }
    }
}
