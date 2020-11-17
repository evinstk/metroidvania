using Microsoft.Xna.Framework;
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

        public HealthComponent(Collider hurtbox)
        {
            _hurtbox = hurtbox;
        }

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
            Insist.IsNotNull(_renderer);
            _originalColor = _renderer.Color;
        }

        public void Update()
        {
            if (Health <= 0)
            {
                Entity.Destroy();
            }
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (local == _hurtbox && local.Entity != other.Entity)
            {
                Health -= 1;
                _renderer.Color = Color.Red;
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
