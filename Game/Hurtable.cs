using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class Hurtable : Component, IUpdatable
    {
        public float StunTime = .5f;
        public float PauseTime = 0.1f;

        public Collider Collider;
        public Action<Hurtable, Collider> OnHurt;
        public FMOD.Studio.EventInstance HurtSound;

        float _stunTimer = 0;
        Color _color;

        SpriteRenderer _renderer;

        public override void Initialize()
        {
            HurtSound = Core.Instance.LoadSound("Common", "impact");
        }

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
            _color = _renderer.Color;
        }

        public override void OnRemovedFromEntity()
        {
            _renderer.Color = _color;
        }

        public void Update()
        {
            if (Collider != null && _stunTimer <= 0 && Collider.CollidesWithAny(out var hit))
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
                        _color = _renderer.Color;
                        _renderer.Color = Color.Transparent;
                    }
                    else
                    {
                        _renderer.Color = _color;
                    }
                }
                if (_stunTimer <= 0)
                    _renderer.Color = _color;
            }
        }

        void Hurt(Collider attacker)
        {
            Timer.PauseFor(PauseTime);
            _stunTimer = StunTime;
            if (HurtSound.isValid())
                HurtSound.start();
            OnHurt?.Invoke(this, attacker);
        }
    }
}
