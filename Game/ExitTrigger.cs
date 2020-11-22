using Nez;

namespace Game
{
    class ExitTrigger : Component, IUpdatable
    {
        string _transitionSrc;
        string _spawn;

        Collider _collider;
        Collider _playerCollider;
        static bool _transitioning = false;

        public ExitTrigger(string transitionSrc, string spawn)
        {
            _transitionSrc = transitionSrc;
            _spawn = spawn;
        }

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            Insist.IsNotNull(_collider);

            _playerCollider = Entity.Scene.FindEntity("player").GetComponent<Collider>();
            Insist.IsNotNull(_playerCollider);
        }

        void SetTransitioningOff()
        {
            _transitioning = false;
        }

        public void Update()
        {
            if (!_transitioning && _collider.CollidesWith(_playerCollider, out _))
            {
                _transitioning = true;
                var transition = Game.Transition(_transitionSrc, _spawn);
                transition.OnTransitionCompleted += SetTransitioningOff;
            }
        }
    }
}
