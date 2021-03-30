using Nez;

namespace Game.Cinema
{
    class PlayerActivator : Component, IUpdatable
    {
        public Collider Collider;
        public string VirtualCameraName;
        public bool Active { get; private set; }

        public void Update()
        {
            var player = Entity.Scene.FindEntity("player");
            if (player == null) return;

            Active = Collider.Bounds.Contains(player.Position);
        }
    }
}
