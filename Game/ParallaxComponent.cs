using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class ParallaxComponent : Component, IUpdatable
    {
        public Vector2 ParallaxScale = Vector2.One;

        Vector2 _basePosition;

        public override void OnAddedToEntity()
        {
            _basePosition = Entity.Position;
        }

        public void Update()
        {
            var cameraPos = Entity.Scene.Camera.Position;
            Entity.Position = _basePosition + new Vector2(_basePosition.X - cameraPos.X, 0) * ParallaxScale;
        }
    }
}
