using Nez;

namespace Game.Cinema
{
    class EntityVcamActivator : Component, IUpdatable
    {
        public Collider Collider;
        public string VirtualCameraName;
        public string ActivatorName = "player";
        public bool Active { get; private set; }

        public void Update()
        {
            var player = Entity.Scene.FindEntity(ActivatorName);
            if (player == null) return;

            Active = Collider.Bounds.Contains(player.Position);
        }
    }
}
