using Game.Editor.Prefab;
using Game.Platformer;
using Microsoft.Xna.Framework;
using Nez;
using System;
using static Game.Editor.Prefab.PlayerMovementData;

namespace Game.Sentry
{
    class AiSentryControllerData : DataComponent
    {
        public int CastDistance = 125;
        public int StopDistance = 25;
        public HitData AttackMask = new HitData();

        public float ProjectileTimeout = 1f;
        public float NoTargetTimeout = 2f;

        public override void AddToEntity(Entity entity)
        {
            var controller = entity.AddComponent<AiSentryController>();
            controller.CastDistance = CastDistance;
            controller.StopDistance = StopDistance;
            controller.AttackMask = AttackMask.HitMask;

            controller.ProjectileTimeout = ProjectileTimeout;
            controller.NoTargetTimeout = NoTargetTimeout;
        }
    }

    class AiSentryController : Component, IUpdatable
    {
        public int CastDistance = 125;
        public int StopDistance = 25;
        public int AttackMask = 0;

        public float ProjectileTimeout = 1f;
        public float NoTargetTimeout = 2f;

        static int[] _dirs = new int[] { -1, 1 };

        PlatformerMovement _movement;
        Spawner _projectileSpawner;
        float _timeout;
        float _noTargetElapsed;

        public override void OnAddedToEntity()
        {
            _movement = Entity.GetComponentStrict<PlatformerMovement>();
            _projectileSpawner = Entity.GetComponentStrict<Spawner>();
        }

        public void Update()
        {
            var position = Entity.Position;
            var mask = AttackMask;
            // cast for terrain layer too in case there's a wall between
            Flags.SetFlag(ref mask, PhysicsLayer.Terrain);

            int fireDir = 0;
            foreach (var dir in _dirs)
            {
                var stopHit = Physics.Linecast(position, position + new Vector2(StopDistance * dir, 0), mask);
                if (stopHit.Collider != null
                    && !Flags.IsUnshiftedFlagSet(stopHit.Collider.PhysicsLayer, PhysicsLayer.Terrain)
                    && dir == _movement.Facing)
                {
                    fireDir = dir;
                    break;
                }
                if (_movement.CurrentState is GroundState state)
                {
                    var hit = Physics.Linecast(position, position + new Vector2(CastDistance * dir, 0), mask);
                    if (hit.Collider != null && !Flags.IsUnshiftedFlagSet(hit.Collider.PhysicsLayer, PhysicsLayer.Terrain))
                    {
                        state.MotionX = dir;
                        break;
                    }
                }
            }

            if (fireDir != 0 && _movement.CurrentState is GroundState groundState)
            {
                groundState.ToTurtle = true;
            }
            else if (_movement.CurrentState is TurtleState turtleState)
            {
                _noTargetElapsed = fireDir == 0 ? Math.Max(_noTargetElapsed - Time.DeltaTime, 0) : NoTargetTimeout;
                if (_noTargetElapsed <= 0)
                {
                    turtleState.ToGround = true;
                }
            }

            _timeout = Math.Max(_timeout - Time.DeltaTime, 0);
            if (fireDir != 0 && _movement.CurrentState is TurtleState && _movement.ElapsedTimeInState >= 2f && _timeout <= 0)
            {
                var projectile = _projectileSpawner.Spawn(_movement.Facing < 0);
                var movement = projectile.GetComponent<ProjectileMovement>();
                movement.Direction.X = fireDir;
                _timeout = ProjectileTimeout;
            }
        }
    }
}
