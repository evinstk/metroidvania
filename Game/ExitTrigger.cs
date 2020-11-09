using FE;
using Nez;

namespace Game
{
    class ExitTrigger : Component, ITriggerListener
    {
        string _transitionSrc;

        Collider _collider;
        Collider _playerCollider;
        static bool _transitioning = false;

        public ExitTrigger(string transitionSrc)
        {
            _transitionSrc = transitionSrc;
        }

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            Insist.IsNotNull(_collider);

            _playerCollider = Entity.Scene.FindEntity("player").GetComponent<Collider>();
        }

        void SetTransitioningOff()
        {
            _transitioning = false;
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (!_transitioning && other == _playerCollider)
            {
                _transitioning = true;
                var transition = Core.StartSceneTransition(new FadeTransition(() => new MainScene(_transitionSrc)));
                transition.FadeOutDuration = 0.3f;
                transition.FadeInDuration = 0.2f;
                transition.OnTransitionCompleted += SetTransitioningOff;
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
