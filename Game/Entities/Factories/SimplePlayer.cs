﻿using Game.Components;
using Microsoft.Xna.Framework;
using Nez;
using System;

namespace Game.Entities.Factories
{
    [EntityDef("simple_player")]
    class SimplePlayer : EntityFactory
    {
        protected override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("simple_player", position);

            var renderer = entity.AddComponent(new PrototypeSpriteRenderer(16, 16));
            renderer.Color = Color.Black;
            renderer.RenderLayer = -10;

            var collider = entity.AddComponent(new BoxCollider(16, 16));
            collider.PhysicsLayer = Mask.Player;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = collider;

            SimplePlatformerController.AddToEntity(entity, ogmoEntity);

            return entity;
        }
    }
}
