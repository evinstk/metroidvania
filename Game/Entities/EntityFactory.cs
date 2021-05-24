using Engine;
using Game.Components;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Entities
{
    abstract class EntityFactory
    {
        public Entity Instantiate(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Create(ogmoEntity, position);

            var props = entity.AddComponent<EntityProperties>();
            props.Properties = ogmoEntity.values;

            return entity;
        }

        protected abstract Entity Create(OgmoEntity ogmoEntity, Vector2 position);
    }
}
