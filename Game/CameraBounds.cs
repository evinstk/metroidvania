using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class CameraBounds : Component, IUpdatable
    {
        public override void OnAddedToEntity()
        {
            SetUpdateOrder(int.MaxValue);
        }

        public void Update()
        {
            var roomBounds = new RectangleF(0, 0, Constants.ResWidth, Constants.ResHeight);
            var cameraBounds = Entity.Scene.Camera.Bounds;

            if (cameraBounds.Top < roomBounds.Top)
                Entity.Scene.Camera.Position += new Vector2(0, roomBounds.Top - cameraBounds.Top);
            if (cameraBounds.Left < roomBounds.Left)
                Entity.Scene.Camera.Position += new Vector2(roomBounds.X - cameraBounds.Left, 0);
            if (cameraBounds.Bottom > roomBounds.Bottom)
                Entity.Scene.Camera.Position += new Vector2(0, roomBounds.Bottom - cameraBounds.Bottom);
            if (cameraBounds.Right > roomBounds.Right)
                Entity.Scene.Camera.Position += new Vector2(roomBounds.Right - cameraBounds.Right, 0);
        }
    }
}
