using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using Nez.UI;

namespace Game
{
    class Hud : RenderableComponent, IUpdatable
    {
        public override float Width => Constants.ResWidth;
        public override float Height => Constants.ResHeight;

        public Vector2 HealthOffset = new Vector2(10, 10);
        public Vector2 EquipmentOffset = new Vector2(10, -10);
        public int HealthSpacing = 15;
        public float IncomingInventoryTimeout = 3;

        NezSpriteFont _font;
        ScriptVars _scriptVars;
        Sprite _fullHeart;
        Sprite _emptyHeart;
        Sprite _iconFrame;

        Table _incomingInventory;
        float _incomingTimer;

        public override void OnAddedToEntity()
        {
            _font = Constants.DefaultFont;
            _scriptVars = Entity.Scene.GetScriptVars();

            _fullHeart = GameContent.LoadSprite("hud", "fullHeart", Core.Content);
            _emptyHeart = GameContent.LoadSprite("hud", "emptyHeart", Core.Content);
            _iconFrame = GameContent.LoadSprite("hud", "iconFrame", Core.Content);

            var playerInventory = Entity.Scene.GetPlayerInventory();
            playerInventory.OnItemAdd += HandleItemAdd;

            var canvas = Entity.AddComponent<UICanvas>();
            canvas.RenderLayer = RenderLayer;

            _incomingInventory = canvas.Stage.AddElement(new Table());
            _incomingInventory.Bottom().Right().Pad(8);
            _incomingInventory.FillParent = true;
        }

        public override void OnRemovedFromEntity()
        {
            var playerInventory = Entity.Scene.GetPlayerInventory();
            playerInventory.OnItemAdd -= HandleItemAdd;
        }

        void HandleItemAdd(Item item)
        {
            var table = new Table();
            table.SetBackground(new NinePatchDrawable(
                new NinePatchSprite(GameContent.LoadSprite("hud", "buttonUp", Core.Content), 1, 1, 1, 1)));
            table.Defaults().Pad(2);

            var icon = new Table();
            icon.SetBackground(new NinePatchDrawable(
                new NinePatchSprite(GameContent.LoadSprite("hud", "buttonUp", Core.Content), 1, 1, 1, 1)));
            var iconSprite = item.Icon;
            if (iconSprite != null)
                icon.Add(new Image(new SpriteDrawable(iconSprite)));
            table.Add(icon).Top().Left().Size(16);

            var label = table.Add(item.Name);
            label.Expand().Top().Right();

            var cell = _incomingInventory.Add(table);
            cell.Width(96).Height(32).SetPadTop(4);

            _incomingInventory.Row();

            _incomingTimer = IncomingInventoryTimeout;
        }

        public void Update()
        {
            if (_incomingTimer > 0)
            {
                _incomingTimer -= Time.AltDeltaTime;
                if (_incomingTimer <= 0)
                    _incomingInventory.ClearChildren();
            }
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

            var rangedOffset = equipmentOffset + new Vector2(_iconFrame.SourceRect.Width, 0);
            DrawSprite(batcher, _iconFrame, rangedOffset);
            var equippedRanged = Entity.Scene.GetPlayerInventory().EquippedRangedWeapon;
            if (equippedRanged != null)
            {
                DrawSprite(batcher, equippedRanged.Icon, rangedOffset);
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
