using Nez;

namespace Game.Cinema
{
    class PlayerActivator : Component, IUpdatable
    {
        public Collider Collider;
        public VirtualCamera VirtualCamera;

        public void Update()
        {
            var player = Entity.Scene.FindEntity("player");
            if (player == null) return;

            VirtualCamera.Enabled = Collider.Bounds.Contains(player.Position);
        }
    }
}
