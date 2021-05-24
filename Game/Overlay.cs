using Engine;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game
{
    class Overlay : Component, IUpdatable
    {
        public float TransitionDuration = 0.2f;

        HashSet<MapRenderer> _renderers = new HashSet<MapRenderer>();

        StencilLightRenderer _lightRenderer;

        public override void OnAddedToEntity()
        {
            _lightRenderer = Entity.Scene.GetRenderer<StencilLightRenderer>();
        }

        public void Update()
        {
            var playerCollider = Entity.Scene.FindEntity("player")?.GetComponent<Collider>();
            if (playerCollider == null) return;

            var hit = Physics.OverlapRectangle(playerCollider.Bounds, Mask.Overlay);
            if (hit != null)
            {
                var renderer = hit.GetComponent<MapRenderer>();
                if (!_renderers.Contains(renderer))
                {
                    renderer.TweenColorTo(Color.Transparent, TransitionDuration).SetRecycleTween(false).Start();
                    _lightRenderer.CollidesWithLayers &= ~Mask.Overlay;
                    _renderers.Add(renderer);
                }
            }
            else
            {
                foreach (var renderer in _renderers)
                {
                    renderer.TweenColorTo(Color.White, TransitionDuration).SetRecycleTween(false).Start();
                }
                _lightRenderer.CollidesWithLayers |= Mask.Overlay;
                _renderers.Clear();
            }
        }
    }
}
