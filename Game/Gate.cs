using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Nez;
using Nez.Textures;
using static Game.Editor.Prefab.PlayerMovementData;

namespace Game
{
    [EntityOnly(typeof(GateState))]
    class GateData : DataComponent
    {
        public AnimationData OpenAnimation = new AnimationData();
        public AnimationData CloseAnimation = new AnimationData();

        public override void AddToEntity(Entity entity)
        {
            var gate = entity.AddComponent<Gate>();
            gate.OpenAnimation = OpenAnimation.MakeAnimation();
            gate.CloseAnimation = CloseAnimation.MakeAnimation();
            gate.AddComponent<Animator<ObserverFrame>>();
            gate.AddComponent<SpriteObserver>();
        }
    }

    class GateState : EntityOnlyComponent
    {
        public BooleanReference OpenState = new BooleanReference();

        public override void AddToEntity(Entity entity)
        {
            var gate = entity.GetComponent<Gate>();
            gate.OpenState = OpenState.Dereference();
        }
    }

    class Gate : Component, IUpdatable
    {
        public Animation<ObserverFrame> OpenAnimation;
        public Animation<ObserverFrame> CloseAnimation;
        public BooleanValue OpenState;

        Collider _collider;
        Animator<ObserverFrame> _animator;
        bool _lastState;

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            _animator = Entity.GetComponent<Animator<ObserverFrame>>();
            _lastState = OpenState.RuntimeValue;
        }

        public void Update()
        {
            if (OpenState.RuntimeValue && !_lastState)
            {
                _animator.Play(OpenAnimation, Animator<ObserverFrame>.LoopMode.ClampForever);
            }
            else if (!OpenState.RuntimeValue && _lastState)
            {
                _animator.Play(CloseAnimation, Animator<ObserverFrame>.LoopMode.ClampForever);
            }

            if (!_animator.IsRunning && OpenState.RuntimeValue)
            {
                _collider.Enabled = false;
            }
            else if (!OpenState.RuntimeValue)
            {
                _collider.Enabled = true;
            }
            _lastState = OpenState.RuntimeValue;
        }
    }
}
