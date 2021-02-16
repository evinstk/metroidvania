using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Game
{
    class Hud : RenderableComponent
    {
        public override float Width => Constants.ResWidth;
        public override float Height => Constants.ResHeight;

        NezSpriteFont _font;
        ScriptVars _scriptVars;

        public override void OnAddedToEntity()
        {
            _font = Constants.DefaultFont;
            _scriptVars = Entity.Scene.GetScriptVars();
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var val = _scriptVars.Get<string>(Vars.HudPrompt);
            var prompt = new FontCharacterSource(val);
            var size = _font.MeasureString(val);
            _font.DrawInto(batcher, ref prompt, new Vector2(Constants.ResWidth / 2, Constants.ResHeight - 20),
                Color.White, 0, size / 2, Vector2.One, SpriteEffects.None, 0);
        }
    }
}
