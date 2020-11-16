using Nez;

namespace Game
{
    class HitComponent : Component, ITriggerListener, IUpdatable
    {
        public BoxCollider HitBox { get; }
        bool _pendingHit = false;

        public HitComponent(BoxCollider hitbox)
        {
            HitBox = hitbox;
        }

        public void Update()
        {
            if (_pendingHit)
            {
                Debug.Log("Hit!");
            }
            _pendingHit = false;
        }

        public override void OnEnabled()
        {
            HitBox.SetEnabled(true);
        }

        public override void OnDisabled()
        {
            HitBox.SetEnabled(false);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (local == HitBox && other.Entity != local.Entity)
                _pendingHit = true;
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            if (local == HitBox && other.Entity != local.Entity)
            {
                _pendingHit = false;
            }
        }
    }
}
