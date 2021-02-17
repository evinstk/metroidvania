using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Game
{
    class Hud : RenderableComponent
    {
        public override float Width => Constants.ResWidth;
        public override float Height => Constants.ResHeight;

        public Vector2 HealthOffset = new Vector2(10, 10);
        public Vector2 EquipmentOffset = new Vector2(10, -10);
        public int HealthSpacing = 15;

        NezSpriteFont _font;
        ScriptVars _scriptVars;
        Sprite _fullHeart;
        Sprite _emptyHeart;
        Sprite _iconFrame;

        public override void OnAddedToEntity()
        {
            _font = Constants.DefaultFont;
            _scriptVars = Entity.Scene.GetScriptVars();

            _fullHeart = GameContent.LoadSprite("hud", "fullHeart", Core.Content);
            _emptyHeart = GameContent.LoadSprite("hud", "emptyHeart", Core.Content);
            _iconFrame = GameContent.LoadSprite("hud", "iconFrame", Core.Content);

            // TODO: don't initialize in HUD
            _scriptVars[Vars.PlayerMaxHealth] = 5;
            _scriptVars[Vars.PlayerHealth] = 5;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var maxHealth = _scriptVars.Get<int>(Vars.PlayerMaxHealth);
            var currHealth = _scriptVars.Get<int>(Vars.PlayerHealth);
            for (var i = 0; i < maxHealth; ++i)
            {
                var sprite = i <= currHealth - 1 ? _fullHeart : _emptyHeart;
                DrawSprite(batcher, sprite, HealthOffset + new Vector2(HealthSpacing * i, 0));
            }

            var equipmentOffset = new Vector2(0, Constants.ResHeight - _iconFrame.SourceRect.Height) + EquipmentOffset;
            DrawSprite(batcher, _iconFrame, equipmentOffset);
            var equippedWeapon = Entity.Scene.GetPlayerInventory().EquippedWeapon;
            if (equippedWeapon != null)
            {
                DrawSprite(batcher, equippedWeapon.Icon, equipmentOffset);
            }

            var val = _scriptVars.Get<string>(Vars.HudPrompt);
            var prompt = new FontCharacterSource(val);
            var size = _font.MeasureString(val);
            _font.DrawInto(batcher, ref prompt, new Vector2(Constants.ResWidth / 2, Constants.ResHeight - 20),
                Color.White, 0, size / 2, Vector2.One, SpriteEffects.None, 0);
        }

        void DrawSprite(Batcher batcher, Sprite sprite, Vector2 position)
        {
            batcher.Draw(sprite, position, Color.White, 0f,
                Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }
}
