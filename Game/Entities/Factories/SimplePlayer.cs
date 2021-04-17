using Microsoft.Xna.Framework;
using Nez;

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

            return entity;
        }
    }
}
