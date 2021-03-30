using Microsoft.Xna.Framework;
using Nez;

namespace Game.Cinema
{
    class VirtualCamera : Component
    {
        public int Priority = 0;
        public BoxCollider Confiner;
        public string FollowName;

        Vector2 _position;

        public Vector2 CameraPosition
        {
            get
            {
                _position = Entity.Scene.FindEntity(FollowName)?.Position ?? _position;

                var cameraBounds = Entity.Scene.Camera.Bounds;
                cameraBounds.Location = _position - cameraBounds.Size / 2;
                var bounds = Confiner.Bounds;
                if (cameraBounds.Top < bounds.Top)
                    _position += new Vector2(0, bounds.Top - cameraBounds.Top);
                if (cameraBounds.Left < bounds.Left)
                    _position += new Vector2(bounds.X - cameraBounds.Left, 0);
                if (cameraBounds.Bottom > bounds.Bottom)
                    _position += new Vector2(0, bounds.Bottom - cameraBounds.Bottom);
                if (cameraBounds.Right > bounds.Right)
                    _position += new Vector2(bounds.Right - cameraBounds.Right, 0);

                return _position;
            }
        }
    }
}
