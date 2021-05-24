using Engine;
using Game.Components;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game.Entities.Factories
{
    [EntityDef("botw_link")]
    class Link : EntityFactory
    {
        static readonly Dictionary<string, int> _fps = new Dictionary<string, int>
        {
            { "link_idle", 10 },
            { "link_walk", 10 },
        };

        protected override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("botw_link", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content, _fps));
            anim.Play("link_idle");
            anim.RenderLayer = -10;

            var collider = entity.AddComponent(new BoxCollider(16, 32));
            collider.PhysicsLayer = Mask.Player;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = collider;

            var ctrl = entity.AddComponent<LinkController>();
            ctrl.AnimationIdle = "link_idle";
            ctrl.AnimationWalk = "link_walk";
            ctrl.AnimationRun = "link_run";
            ctrl.AnimationAttack = "link_attack";

            return entity;
        }
    }
}
