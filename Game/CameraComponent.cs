using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class CameraComponent : Component, IUpdatable
    {
        Vector2 _topLeft;
        Vector2 _bottomRight;

        public CameraComponent(Vector2 topLeft, Vector2 topRight)
        {
            _topLeft = topLeft;
            _bottomRight = topRight;
        }

        public override void OnAddedToEntity()
        {
            Insist.AreEqual(
                Entity, Entity.Scene.Camera.Entity,
                "Component must be attached to camera entity.");
            Entity.UpdateOrder = int.MaxValue;
        }

        public void Update()
        {
            var bounds = Entity.Scene.Camera.Bounds;

            if (bounds.Top < _topLeft.Y)
                Entity.Position += new Vector2(0, _topLeft.Y - bounds.Top);
            if (bounds.Left < _topLeft.X)
                Entity.Position += new Vector2(_topLeft.X - bounds.Left, 0);
            if (bounds.Bottom > _bottomRight.Y)
                Entity.Position += new Vector2(0, _bottomRight.Y - bounds.Bottom);
            if (bounds.Right > _bottomRight.X)
                Entity.Position += new Vector2(_bottomRight.X - bounds.Right, 0);
        }
    }
}
