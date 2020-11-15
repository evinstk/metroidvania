using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System.Linq;

namespace Game
{
    class MobOptions
    {
        public bool PlayerControlled = false;
        public Color? Color = null;

        public static MobOptions DefaultOptions = new MobOptions();
    }

    static class Mob
    {
        public static Entity MakeMobEntity(string name, string type, MobOptions options = null)
        {
            options = options ?? MobOptions.DefaultOptions;

            var map = (Core.Scene as MainScene).Map;
            Insist.IsNotNull(map);

            var mobData = Core.Scene.Content.Load<Data.Mob[]>("Data/Mobs").First(m => m.Type == type);

            var entity = Core.Scene.CreateEntity(name);

            entity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("terrain")));
            entity.AddComponent<CollisionComponent>();
            var physicsCollider = entity.AddComponent(new BoxCollider(mobData.ColliderSize.X, mobData.ColliderSize.Y));
            entity.AddComponent<SpriteRenderer>();
            entity.AddComponent(new AnimationMachine(mobData.Animator, options.Color));
            if (options.PlayerControlled)
                entity.AddComponent<PlayerController>();
            else
                entity.AddComponent<MobController>();
            var mover = entity.AddComponent(new MobMover(physicsCollider));
            mover.MoveSpeed = mobData.MoveSpeed;

            return entity;
        }
    }
}
