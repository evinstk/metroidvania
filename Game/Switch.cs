using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class Switch : Component, IInteractable, IUpdatable
    {
        public string OffAnimation;
        public string OnAnimation;

        public Action<Switch, bool> OnSwitch;

        public string StateVar;
        bool _lastState;

        SpriteAnimator _animator;

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponent<SpriteAnimator>();
        }

        public void Interact()
        {
            var scriptVars = Entity.Scene.GetScriptVars();
            var val = scriptVars.Get<bool>(StateVar);
            scriptVars.Set(StateVar, !val);
        }

        public void Update()
        {
            var scriptVars = Entity.Scene.GetScriptVars();
            var val = scriptVars.Get<bool>(StateVar);
            _animator.Change(val ? OnAnimation : OffAnimation, SpriteAnimator.LoopMode.ClampForever);

            if (val != _lastState)
                OnSwitch?.Invoke(this, val);

            _lastState = val;
        }
    }
}
