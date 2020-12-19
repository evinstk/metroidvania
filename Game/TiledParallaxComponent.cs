using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class TiledParallaxComponent : Component, IUpdatable
    {
        public Vector2 ScrollScale = new Vector2(1, 0);

        TiledSpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<TiledSpriteRenderer>();
            Insist.IsNotNull(_renderer);
        }

        public void Update()
        {
            var offset = Core.Scene.Camera.Position * ScrollScale;
            _renderer.ScrollX = (int)offset.X;
            _renderer.ScrollY = (int)offset.Y;
        }

        public Entity SetParallaxScale(Vector2 scale)
        {
            ScrollScale = scale;
            return Entity;
        }
    }
}
