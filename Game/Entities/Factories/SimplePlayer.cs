using Game.Components;
using Microsoft.Xna.Framework;
using Nez;
using System;

namespace Game.Entities.Factories
{
    [EntityDef("simple_player")]
    class SimplePlayer : EntityFactory
    {
        public override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("simple_player", position);

            var renderer = entity.AddComponent(new PrototypeSpriteRenderer(16, 16));
            renderer.Color = Color.Red;
            renderer.RenderLayer = -10;

            var collider = entity.AddComponent(new BoxCollider(16, 16));
            collider.PhysicsLayer = 0;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = collider;

            var controller = entity.AddComponent<SimplePlatformerController>();
            var values = ogmoEntity.values;
            if (values != null)
            {
                if (values.ContainsKey("gravity"))
                    controller.Gravity = Convert.ToSingle(values["gravity"]);
                if (values.ContainsKey("move_speed"))
                    controller.MoveSpeed = Convert.ToSingle(values["move_speed"]);
                if (values.ContainsKey("jump_time"))
                    controller.JumpTime = Convert.ToSingle(values["jump_time"]);
                if (values.ContainsKey("jump_speed"))
                    controller.JumpSpeed = Convert.ToSingle(values["jump_speed"]);
            }

            return entity;
        }
    }
}
