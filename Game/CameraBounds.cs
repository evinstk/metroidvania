using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class CameraBounds : Component, IUpdatable
    {
        public RectangleF Bounds;

        public override void OnAddedToEntity()
        {
            SetUpdateOrder(int.MaxValue);
        }

        public void Update()
        {
            var cameraBounds = Entity.Scene.Camera.Bounds;

            if (cameraBounds.Top < Bounds.Top)
                Entity.Scene.Camera.Position += new Vector2(0, Bounds.Top - cameraBounds.Top);
            if (cameraBounds.Left < Bounds.Left)
                Entity.Scene.Camera.Position += new Vector2(Bounds.X - cameraBounds.Left, 0);
            if (cameraBounds.Bottom > Bounds.Bottom)
                Entity.Scene.Camera.Position += new Vector2(0, Bounds.Bottom - cameraBounds.Bottom);
            if (cameraBounds.Right > Bounds.Right)
                Entity.Scene.Camera.Position += new Vector2(Bounds.Right - cameraBounds.Right, 0);
        }
    }
}
