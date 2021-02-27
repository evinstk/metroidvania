using Microsoft.Xna.Framework;
using Nez;
using System;

namespace Game
{
    class Projectile : Component, IUpdatable
    {
        public float TimeToLive = 2f;

        float _timer = 0;
        bool _pendingDestroy = false;

        PlatformerMover _mover;

        public override void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<PlatformerMover>();
        }

        public void Update()
        {
            _timer += Time.DeltaTime;

            if (_pendingDestroy)
                Factory.CreateBoom(Entity.Scene, Entity.Position - new Vector2(Math.Sign(_mover.Speed.X), Math.Sign(_mover.Speed.Y)));
            if (_timer > TimeToLive || _pendingDestroy)
                Entity.Destroy();

            Transform.Rotation = Mathf.Deg2Rad * Mathf.RoundToNearest(Vector2.Zero.AngleBetween(new Vector2(1, 0), _mover.Speed), 45);
        }

        public void OnHurt(Hurtable self, Collider attacker)
        {
            _pendingDestroy = true;
        }
    }
}
