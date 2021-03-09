using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class Goblin : Component, IUpdatable
    {
        enum States
        {
            Normal,
        }

        States _state = States.Normal;
        float _timer;
        int _facing = 1;
        Vector2 _moveDest;

        SpriteAnimator _anim;
        PlatformerMover _mover;

        public override void OnAddedToEntity()
        {
            _anim = Entity.GetComponent<SpriteAnimator>();
            _mover = Entity.GetComponent<PlatformerMover>();
        }

        public void Update()
        {
            _timer += Time.DeltaTime;
            _anim.FlipX = _facing < 0;
        }

        void SetState(States state)
        {
            _timer = 0;
            _state = state;
        }

        #region Hurtable

        public void OnHurt(Hurtable self, Collider attacker)
        {
            // TODO: implement
        }

        #endregion
    }
}
