using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game
{
    class Overlay : Component, ITriggerListener
    {
        public float TransitionDuration = 0.2f;

        bool _hidden = false;

        HashSet<Pair<Collider>> _entered = new HashSet<Pair<Collider>>();

        MapRenderer _renderer;
        MapCollider _mapCollider;
        StencilLightRenderer _lightRenderer;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponentStrict<MapRenderer>();
            _mapCollider = Entity.GetComponentStrict<MapCollider>();
            _lightRenderer = Entity.Scene.GetRenderer<StencilLightRenderer>();
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            _entered.Add(new Pair<Collider>(other, local));
            UpdateVisibility();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            _entered.Remove(new Pair<Collider>(other, local));
            UpdateVisibility();
        }

        void UpdateVisibility()
        {
            if (!_hidden && _entered.Count > 0)
            {
                _renderer.TweenColorTo(Color.Transparent, TransitionDuration).SetRecycleTween(true).Start();
                _lightRenderer.CollidesWithLayers ^= _mapCollider.PhysicsLayer;
                _hidden = true;
            }
            if (_hidden && _entered.Count == 0)
            {
                _renderer.TweenColorTo(Color.White, TransitionDuration).SetRecycleTween(true).Start();
                _lightRenderer.CollidesWithLayers ^= _mapCollider.PhysicsLayer;
                _hidden = false;
            }
        }
    }
}
