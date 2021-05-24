using Engine;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game.Entities.Factories
{
    [EntityDef("keese")]
    class Keese : EntityFactory
    {
        static Dictionary<string, int> _fps = new Dictionary<string, int>
        {
            { "keese_fly", 14 },
        };

        protected override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("keese", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content, _fps));
            anim.Play("keese_fly");
            anim.RenderLayer = -5;

            var collider = entity.AddComponent(new BoxCollider(16, 16));
            collider.PhysicsLayer = Mask.Enemy;

            return entity;
        }
    }
}
