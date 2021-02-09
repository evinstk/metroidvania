using Nez;
using System.Collections.Generic;

namespace Game.Hit
{
    interface IHitListener
    {
        void OnHit();
    }

    class Hitter : Component
    {
        public int HitMask = -1;

        Animator<ObserverFrame> _animator;
        Collider[] _colliderResults = new Collider[8];
        List<IHitListener> _tempHitList = new List<IHitListener>();

        ColliderTriggerHelper _triggerHelper;

        public override void Initialize()
        {
            _triggerHelper = new ColliderTriggerHelper(Entity);
        }

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponentStrict<Animator<ObserverFrame>>();
            _animator.OnFrameEnter += HandleFrameEnter;
        }

        public override void OnRemovedFromEntity()
        {
            _animator.OnFrameEnter -= HandleFrameEnter;
        }

        void HandleFrameEnter(ObserverFrame frame)
        {
            for (var i = 0; i < frame.HitBoxes.Length; ++i)
            {
                var hit = Entity.Scene.CreateEntity("hit");

                var hitbox = frame.HitBoxes[i];
                var location = frame.HitBoxes[i].Location;
                if (frame.Flip) location.X = -location.X - frame.HitBoxes[i].Width;
                hitbox.Location = location + Entity.Position;

                var collider = hit.AddComponent(new BoxCollider(hitbox));
                collider.PhysicsLayer = 0;
                collider.CollidesWithLayers = HitMask;
                collider.IsTrigger = true;

                var triggerHelper = new ColliderTriggerHelper(hit);
                triggerHelper.Update();

                hit.Destroy();
            }
        }
    }
}
