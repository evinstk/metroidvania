using Nez;

namespace Game
{
    class Projectile : Component, IUpdatable
    {
        public float TimeToLive = 2f;

        float _timer = 0;
        bool _pendingDestroy = false;

        public void Update()
        {
            _timer += Time.DeltaTime;

            if (_timer > TimeToLive || _pendingDestroy)
                Entity.Destroy();
        }

        public void OnHurt(Hurtable self, Collider attacker)
        {
            _pendingDestroy = true;
        }
    }
}
