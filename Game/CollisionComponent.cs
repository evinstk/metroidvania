using Nez;
using static Nez.Tiled.TiledMapMover;

namespace Game
{
    class CollisionComponent : Component
    {
        public CollisionState Collision { get; } = new CollisionState();
    }
}
