using Engine;
using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;

namespace Game.Entities
{
    class EntityLoader : SceneComponent
    {
        Dictionary<string, EntityFactory> _factories = new Dictionary<string, EntityFactory>();

        public EntityLoader()
        {
            var factoryTypes = ReflectionUtils.GetAllSubclasses(typeof(EntityFactory), true);
            foreach (var type in factoryTypes)
            {
                var entityDef = type.GetAttribute<EntityDefAttribute>();
                if (entityDef != null)
                    _factories[entityDef.Name] = (EntityFactory)Activator.CreateInstance(type);
            }
        }

        public Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            if (_factories.TryGetValue(ogmoEntity.name, out var factory))
                return factory.Instantiate(ogmoEntity, position);
            Debug.Log($"No entity type {ogmoEntity.name}");
            return null;
        }
    }
}
