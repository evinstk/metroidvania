using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class RectangleRenderer : RenderableComponent
    {
        public override float Width => _width;
        float _width;
        public override float Height => _height;
        float _height;
        Color _color;

        public RectangleRenderer(Color color, float width = 32, float height = 32)
        {
            _color = color;
            _width = width;
            _height = height;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            batcher.DrawRect(Entity.Position, _width, _height, _color);
        }
    }
}
