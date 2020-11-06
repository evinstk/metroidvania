using Nez;

namespace Game
{
    class ExitTrigger : Component, IUpdatable
    {
        Collider _collider;
        Collider _playerCollider;

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            Insist.IsNotNull(_collider);

            _playerCollider = Entity.Scene.FindEntity("player").GetComponent<Collider>();
        }

        public void Update()
        {
            if (_collider.CollidesWith(_playerCollider, out _))
            {
                Debug.Log("Trigger exit.");
            }
        }
    }
}
