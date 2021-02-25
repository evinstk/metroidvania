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
        public SoundEffect HurtSound;

        float _stunTimer = 0;
        Color _mColor;

        SpriteRenderer _renderer;

        public override void Initialize()
        {
            HurtSound = Core.Scene.Content.LoadSoundEffect($"{ContentPath.Sounds}impact.wav");
        }

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public override void OnRemovedFromEntity()
        {
            _renderer.Color = _mColor;
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
                    _renderer.Color = _mColor;
            }
        }

        void Hurt(Collider attacker)
        {
            Timer.PauseFor(PauseTime);
            _stunTimer = StunTime;
            HurtSound?.Play();
            OnHurt(this, attacker);
        }
    }
}
