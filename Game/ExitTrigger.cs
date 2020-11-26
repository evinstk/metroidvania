using Nez;

namespace Game
{
    class ExitTrigger : Component, IUpdatable
    {
        string _transitionSrc;
        string _spawn;

        Collider _collider;
        Collider _playerCollider;
        HealthComponent _health;

        public ExitTrigger(string transitionSrc, string spawn)
        {
            _transitionSrc = transitionSrc;
            _spawn = spawn;
        }

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            Insist.IsNotNull(_collider);

            var player = Entity.Scene.FindEntity("player");
            _playerCollider = player.GetComponent<Collider>();
            Insist.IsNotNull(_playerCollider);

            _health = player.GetComponent<HealthComponent>();
            Insist.IsNotNull(_health);
        }

        public void Update()
        {
            if (_collider.CollidesWith(_playerCollider, out _))
            {
                var scene = Entity.Scene as MainScene;
                scene.Transition(_transitionSrc, _spawn);
            }
        }
    }
}
