using Microsoft.Xna.Framework;
using Nez;

namespace Game.Entities
{
    abstract class EntityFactory
    {
        public abstract Entity Create(OgmoEntity ogmoEntity, Vector2 position);
    }
}
