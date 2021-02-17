using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class Hurtable : Component, IUpdatable
    {
        public float StunTime = .5f;

        public Collider Collider;
        public Action<Hurtable, Collider> OnHurt;

        float _stunTimer = 0;
        Color _mColor;

        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public void Update()
        {
            if (Collider != null && OnHurt != null && _stunTimer <= 0 && Collider.CollidesWithAny(out var hit))
            {
                Hurt(hit.Collider);
            }

            if (_stunTimer > 0)
            {
                _stunTimer -= Time.DeltaTime;
                if (Timer.OnInterval(.05f))
                {
                    if (_renderer.Color != Color.Transparent)
                    {
                        _mColor = _renderer.Color;
                        _renderer.Color = Color.Transparent;
                    }
                    else
                    {
                        _renderer.Color = _mColor;
                    }
                }
                if (_stunTimer <= 0)
                    _renderer.Enabled = true;
            }
        }

        void Hurt(Collider attacker)
        {
            Timer.PauseFor(0.1f);
            _stunTimer = StunTime;
            OnHurt(this, attacker);
        }
    }
}
