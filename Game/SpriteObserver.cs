using Nez;
using Nez.Sprites;

namespace Game
{
    class SpriteObserver : Component
    {
        Animator<ObserverFrame> _animator;
        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponentStrict<Animator<ObserverFrame>>();
            _animator.OnFrameEnter += HandleFrameEnter;
            _renderer = Entity.GetComponentStrict<SpriteRenderer>();
        }

        public override void OnRemovedFromEntity()
        {
            _animator.OnFrameEnter -= HandleFrameEnter;
        }

        void HandleFrameEnter(ObserverFrame frame)
        {
            _renderer.Sprite = frame.Sprite;
            _renderer.FlipX = frame.Flip;
        }
    }
}
