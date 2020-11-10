using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    class AnimationComponent : Component, IUpdatable
    {
        string _animationGroup;

        SpriteAnimator _animator;
        ControllerComponent _controller;
        Dictionary<string, bool> _flips;

        public AnimationComponent(string animationGroup)
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
                    return Sprite.SpritesFromAtlas(texture, a.CellWidth, a.CellHeight);
                });

            _animator = Entity.AddComponent<SpriteAnimator>();
            _controller = Entity.GetComponent<ControllerComponent>();
            Insist.IsNotNull(_controller);

            foreach (var animType in group.AnimationTypes)
            {
                var anim = animations[animType.Animation];
                var sprites2 = atlases[anim.SpriteAtlas];
                _animator.AddAnimation(
                    animType.Type,
                    Enumerable.Range(anim.CellStart, anim.CellCount).Select(i => sprites2[i]).ToArray());
            }

            _flips = group.AnimationTypes.ToDictionary(t => t.Type, t => t.Flip);

            _animator.Play("IdleRight");
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
                _animator.Play(animation);
                _animator.FlipX = _flips[animation];
            }
        }
    }
}
