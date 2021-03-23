using MoonSharp.Interpreter;
using Nez;
using Nez.Sprites;
using System;

namespace Game
{
    class Switch : Component, IUpdatable
    {
        public string TurningOff;
        public string TurningOn;
        public string Off;
        public string On;

        public Action<Switch, bool> OnSwitch;

        public string StateVar;
        public bool UseFunction;

        public Func<Switch, bool> Condition;

        bool _lastState;

        SpriteAnimator _animator;

        public override void OnAddedToEntity()
        {
            Insist.IsTrue(
                (StateVar == null || Condition == null)
                && (StateVar != null || Condition != null),
                "Switch component should only have either StateVar or Condition set but not both.");

            _animator = Entity.GetComponent<SpriteAnimator>();

            _lastState = GetStateValue();
            _animator.Change(_lastState ? On : Off);
        }

        public void Update()
        {
            var scriptVars = Entity.Scene.GetScriptVars();

            var val = GetStateValue();

            if (!_animator.IsRunning)
                _animator.Play(val ? On : Off);

            if (val != _lastState)
            {
                OnSwitch?.Invoke(this, val);
                _animator.Play(val ? TurningOn : TurningOff, SpriteAnimator.LoopMode.Once);
            }

            _lastState = val;
        }

        bool GetStateValue()
        {
            if (Condition != null)
                return Condition.Invoke(this);

            var scriptVars = Entity.Scene.GetScriptVars();
            if (UseFunction)
            {
                var stateFn = scriptVars.Get<Closure>(StateVar);
                return stateFn.Call().CastToBool();
            }
            else
            {
                return scriptVars.Get<bool>(StateVar);
            }
        }
    }
}
