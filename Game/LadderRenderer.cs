using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;

namespace Game
{
    class LadderRenderer : RenderableComponent
    {
        public Sprite Sprite;

        public override float Width => 16f;
        public override float Height => _height;

        int _height;

        public LadderRenderer(Sprite sprite, int height)
        {
            Sprite = sprite;
            _height = height;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var position = Entity.Position;
            var steps = _height / 16;
            for (var i = 0; i < steps; ++i)
            {
                batcher.Draw(Sprite, position + new Vector2(0, i * 16), Color.White, 0, Vector2.Zero, 1, 0, _layerDepth);
            }
        }
    }
}
