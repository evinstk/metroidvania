﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;
using System.Linq;
using static Nez.Sprites.SpriteAnimator;

namespace Game.Animator
{
    class AnimatorContext
    {
        public SpriteAnimator SpriteAnimator;
        public ControllerComponent Controller;
        public BoxCollider Collider;
        public Dictionary<string, AnimationMeta> Meta;

        public void PlayAnimation(string animation, LoopMode? loopMode = null)
        {
            SpriteAnimator.Play(animation, loopMode);
            var meta = Meta[animation];
            SpriteAnimator.FlipX = meta.Flip;
            var size = meta.ColliderSize;
            Collider.SetSize(size.X, size.Y);
        }
    }

    class AnimationMeta
    {
        public bool Flip;
        public Vector2 ColliderSize;
    }

    class Animator : Component, IUpdatable
    {
        string _animationGroup;
        StateMachine<AnimatorContext> _fsm;

        public Animator(string animationGroup)
        {
            _animationGroup = animationGroup;
        }

        public override void OnAddedToEntity()
        {
            var group = Entity.Scene.Content.Load<Data.AnimationGroup[]>("Data/AnimationGroups").First(g => g.Name == _animationGroup);
            var animations = Entity.Scene.Content.Load<Data.Animation[]>("Data/Animations").ToDictionary(a => a.Name);
            var atlases = Entity.Scene.Content.Load<Data.SpriteAtlas[]>("Data/SpriteAtlases").ToDictionary(
                a => a.Name, a =>
                {
                    var texture = Entity.Scene.Content.LoadTexture("Textures/" + a.Texture);
                    var sprites = Sprite.SpritesFromAtlas(texture, a.CellWidth, a.CellHeight);
                    return sprites;
                });

            var spriteAnimator = Entity.AddComponent<SpriteAnimator>();
            var controller = Entity.GetComponent<ControllerComponent>();
            Insist.IsNotNull(controller);
            var collider = Entity.GetComponent<BoxCollider>();
            Insist.IsNotNull(collider);

            foreach (var animType in group.AnimationTypes)
            {
                var anim = animations[animType.Animation];
                var sprites = atlases[anim.SpriteAtlas];
                spriteAnimator.AddAnimation(
                    animType.Type,
                    Enumerable.Range(anim.CellStart, anim.CellCount).Select(i => sprites[i]).ToArray(),
                    12);
            }

            var meta = group.AnimationTypes.ToDictionary(t => t.Type, t =>
            {
                var animation = animations[t.Animation];
                return new AnimationMeta
                {
                    Flip = t.Flip,
                    ColliderSize = animation.ColliderSize?.ToVector2() ?? atlases[animation.SpriteAtlas][0].SourceRect.GetSize().ToVector2(),
                };
            });

            _fsm = new StateMachine<AnimatorContext>(new AnimatorContext
            {
                SpriteAnimator = spriteAnimator,
                Controller = controller,
                Collider = collider,
                Meta = meta,
            }, new IdleState());
            _fsm.AddState(new WalkState());
            _fsm.AddState(new AttackState());
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
            _context.PlayAnimation(_context.Controller.Facing > 0
                ? "IdleRight" : "IdleLeft");
        }

        public override void Update(float deltaTime)
        {
            if (_context.Controller.Velocity.X != 0)
            {
                _machine.ChangeState<WalkState>();
            }
            if (_context.Controller.AttackInput)
            {
                _machine.ChangeState<AttackState>();
            }
        }
    }

    class WalkState : State<AnimatorContext>
    {
        public override void Begin()
        {
            _context.PlayAnimation(_context.Controller.Facing > 0
                ? "WalkRight" : "WalkLeft");
        }

        public override void Update(float deltaTime)
        {
            if (_context.Controller.Velocity.X == 0 && _context.Controller.Collision.Below)
            {
                _machine.ChangeState<IdleState>();
            }
            if (_context.Controller.AttackInput)
            {
                _machine.ChangeState<AttackState>();
            }
        }
    }

    class AttackState : State<AnimatorContext>
    {
        public override void Begin()
        {
            _context.PlayAnimation(_context.Controller.Facing > 0
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
}
