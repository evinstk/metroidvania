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
            Flags.SetFlagExclusive(ref physicsCollider.CollidesWithLayers, Layer.Default);
            var renderer = entity.AddComponent<SpriteRenderer>();
            renderer.Color = options.Color ?? Color.White;
            if (options.PlayerControlled)
            {
                entity.AddComponent<PlayerController>();
            }
            else if (mobData.AiType != null)
            {
                switch (mobData.AiType)
                {
                    case "Attacker":
                        entity.AddComponent<AttackerController>();
                        break;
                    default:
                        throw new System.Exception("Unsupported AI type");
                }
            }
            var mover = entity.AddComponent(new MobMover(physicsCollider));
            mover.MoveSpeed = mobData.MoveSpeed;

            var hurtbox = entity.AddComponent(new BoxCollider(mobData.ColliderSize.X / 2, mobData.ColliderSize.Y / 2));
            hurtbox.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtbox.PhysicsLayer, Layer.HurtBox);
            var healthC = entity.AddComponent(new HealthComponent(hurtbox));
            healthC.Health = mobData.Health;

            var hitbox = entity.AddComponent<BoxCollider>();
            Flags.SetFlagExclusive(ref hitbox.CollidesWithLayers, Layer.HurtBox);
            var hitboxC = entity.AddComponent(new HitBoxComponent(hitbox));
            hitboxC.SetEnabled(false);

            entity.AddComponent(new AnimationMachine(mobData.Animator));

            return entity;
        }
    }
}
