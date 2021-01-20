using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game.Hit
{
    interface IHitListener
    {
        void OnHit();
    }

    class Hitter :
#if DEBUG
        RenderableComponent
#else
        Component
#endif
    {
        public int HitMask = -1;

        Animator<ObserverFrame> _animator;
        Collider[] _colliderResults = new Collider[8];
        List<IHitListener> _tempHitList = new List<IHitListener>();

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponentStrict<Animator<ObserverFrame>>();
            _animator.OnFrameEnter += HandleFrameEnter;
        }

        public override void OnRemovedFromEntity()
        {
            _animator.OnFrameEnter -= HandleFrameEnter;
        }

        void HandleFrameEnter(ObserverFrame frame)
        {
            for (var i = 0; i < frame.HitBoxes.Length; ++i)
            {
                CastHitBox(frame.HitBoxes[i], frame.Flip);
            }
#if DEBUG
            _flip = frame.Flip;
            _hitboxes = frame.HitBoxes;
#endif
        }

        void CastHitBox(RectangleF hitbox, bool flip)
        {
            var location = hitbox.Location;
            if (flip) location.X = -location.X - hitbox.Width;
            hitbox.Location = location + Entity.Position;
            Physics.OverlapRectangleAll(ref hitbox, _colliderResults, HitMask);
            for (var i = 0; i < _colliderResults.Length; ++i)
            {
                var collider = _colliderResults[i];
                if (collider != null)
                    collider.GetComponents(_tempHitList);
                _colliderResults[i] = null;
            }

            foreach (var listener in _tempHitList)
                listener.OnHit();
            _tempHitList.Clear();
        }

#if DEBUG
        RectangleF[] _hitboxes = null;
        bool _flip = false;

        public override float Width => 1f;
        public override float Height => 1f;

        public override void Render(Batcher batcher, Camera camera)
        {
        }

        public override void DebugRender(Batcher batcher)
        {
            if (_hitboxes != null)
            {
                var color = Color.DarkRed;
                color.A = 20;
                for (var i = 0; i < _hitboxes.Length; ++i)
                {
                    var hitbox = _hitboxes[i];
                    var location = hitbox.Location;
                    if (_flip) location.X = -location.X - hitbox.Width;
                    hitbox.Location = location + Entity.Position;
                    batcher.DrawRect(hitbox, color);
                }
            }
        }
#endif
    }
}
