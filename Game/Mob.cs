using Game.MobState;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System.Linq;

namespace Game
{
    class MobOptions
    {
        public bool PlayerControlled = false;
        public Color? Color = null;
        public int StartingHealth = -1;
        public Teams Team = Teams.A;
        public string DialogSrc = null;
        public int RenderLayer = 0;

        public static MobOptions DefaultOptions = new MobOptions();
    }

    static class Mob
    {
        public static Entity MakeMobEntity(string name, string type, MobOptions options = null)
        {
            options = options ?? MobOptions.DefaultOptions;

            var mobData = Core.Scene.Content.Load<Data.Mob[]>("Data/Mobs").First(m => m.Type == type);

            var entity = Core.Scene.CreateEntity(name);

            entity.AddComponent<CollisionComponent>();
            var physicsCollider = entity.AddComponent(new BoxCollider(mobData.ColliderSize.X, mobData.ColliderSize.Y));
            Flags.SetFlagExclusive(ref physicsCollider.CollidesWithLayers, Layer.Doodad);
            var renderer = entity.AddComponent<SpriteRenderer>();
            renderer.Color = options.Color ?? Color.White;
            renderer.RenderLayer = options.RenderLayer;
            if (options.PlayerControlled)
            {
                entity.AddComponent<PlayerController>();
                entity.AddComponent<InteractionComponent>();
            }
            else if (mobData.AiType != null)
            {
                switch (mobData.AiType)
                {
                    case "Attacker":
                        var attack = entity.AddComponent<AttackerController>();
                        attack.Team = options.Team;
                        break;
                    default:
                        throw new System.Exception("Unsupported AI type");
                }
            }
            else
            {
                entity.AddComponent<FreeController>();
            }

            var hurtboxEntity = Core.Scene.CreateEntity(name + "-hurtbox");
            hurtboxEntity.Parent = entity.Transform;
            var hurtbox = hurtboxEntity.AddComponent(new BoxCollider(mobData.ColliderSize.X / 2, mobData.ColliderSize.Y / 2));
            hurtbox.IsTrigger = true;
            Flags.SetFlagExclusive(ref hurtbox.PhysicsLayer, Layer.GetTeamHurtBox(options.Team));
            var healthC = hurtboxEntity.AddComponent(new HealthComponent(hurtbox));
            healthC.Health = options.StartingHealth != -1 ? options.StartingHealth : mobData.Health;

            var hitboxEntity = Core.Scene.CreateEntity(name + "-hitbox");
            hitboxEntity.Parent = entity.Transform;
            var hitbox = hitboxEntity.AddComponent<BoxCollider>();
            hitbox.CollidesWithLayers = Layer.GetOtherTeamsMask(options.Team);
            var hitboxC = entity.AddComponent(new HitBoxComponent(hitbox));
            hitboxC.SetEnabled(false);

            entity.AddAnimator(mobData.Animator);
            entity.AddComponent(new MobStateMachine(physicsCollider)).SetMoveSpeed(mobData.MoveSpeed);

            if (options.DialogSrc != null)
            {
                entity.AddComponent(new DialogComponent(options.DialogSrc));
            }

            return entity;
        }
    }
}
