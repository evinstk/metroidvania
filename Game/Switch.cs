using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class Switch : Component, IInteractable, IUpdatable
    {
        public string TurningOff;
        public string TurningOn;
        public string Off;
        public string On;

        public Action<Switch, bool> OnSwitch;

        public string StateVar;
        bool _lastState;

        SpriteAnimator _animator;

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponent<SpriteAnimator>();

            var scriptVars = Entity.Scene.GetScriptVars();
            _lastState = scriptVars.Get<bool>(StateVar);
            _animator.Change(_lastState ? On : Off);
        }

        public void Interact(Entity interactor)
        {
            var scriptVars = Entity.Scene.GetScriptVars();
            var val = scriptVars.Get<bool>(StateVar);
            scriptVars.Set(StateVar, !val);
        }

        public void Update()
        {
            var scriptVars = Entity.Scene.GetScriptVars();
            var val = scriptVars.Get<bool>(StateVar);
            //_animator.Change(val ? TurningOn : TurningOff, SpriteAnimator.LoopMode.ClampForever);
            if (!_animator.IsRunning)
                _animator.Play(val ? On : Off);

            if (val != _lastState)
            {
                OnSwitch?.Invoke(this, val);
                _animator.Play(val ? TurningOn : TurningOff, SpriteAnimator.LoopMode.Once);
            }

            _lastState = val;
        }
    }
}
