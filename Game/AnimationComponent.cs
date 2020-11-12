using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    class AnimationMeta
    {
        public bool Flip;
        public Vector2 ColliderSize;
    }

    class AnimationComponent : Component, IUpdatable
    {
        string _animationGroup;

        BoxCollider _collider;

        SpriteAnimator _animator;
        ControllerComponent _controller;
        Dictionary<string, AnimationMeta> _meta;

        public AnimationComponent(string animationGroup)
        {
            _animationGroup = animationGroup;
        }

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<BoxCollider>();
            Insist.IsNotNull(_collider);

            var group = Entity.Scene.Content.Load<Data.AnimationGroup[]>("Data/AnimationGroups").First(g => g.Name == _animationGroup);
            var animations = Entity.Scene.Content.Load<Data.Animation[]>("Data/Animations").ToDictionary(a => a.Name);
            var atlases = Entity.Scene.Content.Load<Data.SpriteAtlas[]>("Data/SpriteAtlases").ToDictionary(
                a => a.Name, a =>
                {
                    var texture = Entity.Scene.Content.LoadTexture("Textures/" + a.Texture);
                    var sprites = Sprite.SpritesFromAtlas(texture, a.CellWidth, a.CellHeight);
                    return sprites;
                });

            _animator = Entity.AddComponent<SpriteAnimator>();
            _controller = Entity.GetComponent<ControllerComponent>();
            Insist.IsNotNull(_controller);

            foreach (var animType in group.AnimationTypes)
            {
                var anim = animations[animType.Animation];
                var sprites = atlases[anim.SpriteAtlas];
                _animator.AddAnimation(
                    animType.Type,
                    Enumerable.Range(anim.CellStart, anim.CellCount).Select(i => sprites[i]).ToArray(),
                    12);
            }

            _meta = group.AnimationTypes.ToDictionary(t => t.Type, t =>
            {
                return new AnimationMeta
                {
                    Flip = t.Flip,
                    ColliderSize = animations[t.Animation].ColliderSize.ToVector2(),
                };
            });

            PlayAnimation("IdleRight");
        }

        void PlayAnimation(string animation)
        {
            _animator.Play(animation);
            var meta = _meta[animation];
            _animator.FlipX = meta.Flip;
            var size = meta.ColliderSize;
            _collider.SetSize(size.X, size.Y);
        }

        public void Update()
        {
            string animation = null;

            if (_controller.Velocity.X == 0 && _controller.Collision.Below)
            {
                if (_controller.Facing > 0)
                    animation = "IdleRight";
                else
                    animation = "IdleLeft";
            }

            if (_controller.Velocity.X > 0 && _controller.Collision.Below)
            {
                animation = "WalkRight";
            }

            if (_controller.Velocity.X < 0 && _controller.Collision.Below)
            {
                animation = "WalkLeft";
            }

            if (animation != null && !_animator.IsAnimationActive(animation))
            {
                PlayAnimation(animation);
            }
        }
    }
}
