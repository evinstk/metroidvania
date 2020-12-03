using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;

namespace Game
{
    class HealthComponent : Component, IUpdatable, ITriggerListener
    {
        public int Health;

        Collider _hurtbox;
        SpriteRenderer _renderer;
        Color _originalColor;
        SoundEffect _impactEffect;

        public HealthComponent(Collider hurtbox)
        {
            _hurtbox = hurtbox;
        }

        public override void OnAddedToEntity()
        {
            _impactEffect = Core.Content.LoadSoundEffect("Sounds/impact");
            Insist.IsNotNull(_impactEffect);

            _renderer = Entity.Parent.Entity.GetComponent<SpriteRenderer>();
            Insist.IsNotNull(_renderer);
            _originalColor = _renderer.Color;
        }

        public void Update()
        {
            if (Health <= 0)
            {
                Entity.Parent.Entity.Destroy();
            }
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (local == _hurtbox && local.Entity != other.Entity)
            {
                Health -= 1;
                _renderer.Color = Color.Red;
                _impactEffect.Play();
                _renderer.TweenColorTo(_originalColor)
                    .SetRecycleTween(true)
                    .Start();
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
