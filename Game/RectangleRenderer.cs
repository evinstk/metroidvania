using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class RectangleRenderer : RenderableComponent
    {
        float _width;
        float _height;
        Vector2 _origin;

        public override RectangleF Bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    _bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin,
                        Entity.Transform.Scale, Entity.Transform.Rotation, _width, _height);
                    _areBoundsDirty = false;
                }
                return _bounds;
            }
        }

        public RectangleRenderer(Color color, float width = 32, float height = 32)
        {
            Color = color;
            _width = width;
            _height = height;

            _origin = new Vector2(width / 2, height / 2);
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            batcher.DrawRect(Entity.Position - new Vector2(_width / 2, _height / 2), _width, _height, Color);
        }
    }
}
