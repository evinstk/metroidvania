using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;
using static Game.Editor.Prefab.PlayerMovementData;

namespace Game
{
    class ProjectileMovementData : DataComponent
    {
        public AnimationData ProjectileAnimation = new AnimationData();

        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<ProjectileMovement>();
            var animator = entity.AddComponent<Animator<ObserverFrame>>();
            animator.Play(ProjectileAnimation.MakeAnimation());
            entity.AddComponent<SpriteObserver>();
        }
    }

    class ProjectileMovement : Component, IUpdatable
    {
        public float TimeToLive = 5f;
        public Vector2 Direction = new Vector2(1, 0);
        public float MoveSpeed = 150f;

        float _elapsed = 0f;

        ProjectileMover _mover;

        public override void OnAddedToEntity()
        {
            _mover = Entity.AddComponent<ProjectileMover>();
        }

        public void Update()
        {
            if (Time.DeltaTime == 0) return;

            _elapsed += Time.DeltaTime;
            if (_elapsed >= TimeToLive)
                Entity.Destroy();

            var motion = Direction * MoveSpeed * Time.DeltaTime;
            if (_mover.Move(motion))
                Entity.Destroy();

            Transform.Rotation = Mathf.Deg2Rad * Mathf.RoundToNearest(Vector2.Zero.AngleBetween(new Vector2(1, 0), motion), 45);
        }
    }
}
