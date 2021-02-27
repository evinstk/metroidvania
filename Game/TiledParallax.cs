using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class TiledParallax : Component, IUpdatable
    {
        public TiledSpriteRenderer Renderer;
        public Vector2 ScrollScale = new Vector2(1, 0);

        public void Update()
        {
            var offset = Entity.Scene.Camera.Position * ScrollScale;
            Renderer.ScrollX = (int)offset.X;
            Renderer.ScrollY = (int)offset.Y;
        }
    }
}
