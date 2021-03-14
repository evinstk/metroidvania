using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
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
        public Vector2 SaveIconOffset = new Vector2(-10, -10);
        public int HealthSpacing = 15;
        public float IncomingInventoryTimeout = 3;
        public int ElementSpacing = 4;

        NezSpriteFont _font;
        ScriptVars _scriptVars;
        Sprite _fullHeart;
        Sprite _emptyHeart;
        Sprite _iconFrame;
        NineSliceSpriteRenderer _manaBar;

        Table _incomingInventory;
        float _incomingTimer;

        SpriteAnimator _saveIconAnimator;
        FMOD.Studio.EventInstance _saveSound;

        public override void OnAddedToEntity()
        {
            _font = Constants.DefaultFont;
            _scriptVars = Entity.Scene.GetScriptVars();

            _fullHeart = GameContent.LoadSprite("hud", "fullHeart", Core.Content);
            _emptyHeart = GameContent.LoadSprite("hud", "emptyHeart", Core.Content);
            _iconFrame = GameContent.LoadSprite("hud", "iconFrame", Core.Content);

            var manaBarSprite = new NinePatchSprite(GameContent.LoadSprite("hud", "mana_bar", Core.Content), 2, 2, 2, 2);
            _manaBar = Entity.AddComponent(new NineSliceSpriteRenderer(manaBarSprite));
            _manaBar.RenderLayer = RenderLayers.Hud;
            _manaBar.Height = 8f;

            _saveIconAnimator = Entity.AddComponent(Animator.MakeAnimator("hud", Core.Content));
            _saveIconAnimator.RenderLayer = RenderLayers.Null;

            var playerInventory = Entity.Scene.GetPlayerInventory();
            playerInventory.OnItemAdd += HandleItemAdd;

            var canvas = Entity.AddComponent<UICanvas>();
            canvas.RenderLayer = RenderLayer;

            _incomingInventory = canvas.Stage.AddElement(new Table());
            _incomingInventory.Bottom().Right().Pad(8);
            _incomingInventory.FillParent = true;

            _saveSound = GameContent.LoadSound("Common", "save");

            Core.GetGlobalManager<SaveSystem>().OnSave += OnSave;
        }

        public override void OnRemovedFromEntity()
        {
            var playerInventory = Entity.Scene.GetPlayerInventory();
            playerInventory.OnItemAdd -= HandleItemAdd;

            _saveSound.release();

            Core.GetGlobalManager<SaveSystem>().OnSave -= OnSave;
        }

        public override void OnEnabled()
        {
            _incomingInventory?.SetIsVisible(true);
        }

        public override void OnDisabled()
        {
            _incomingInventory?.SetIsVisible(false);
        }

        public void OnSave(SaveSystem saveSystem)
        {
            _saveIconAnimator.Play("save", SpriteAnimator.LoopMode.ClampForever);
            _saveSound.start();
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

            _manaBar.LocalOffset = HealthOffset + new Vector2(0, _fullHeart.SourceRect.Height + ElementSpacing);
            var maxMana = _scriptVars.Get<int>(Vars.PlayerMaxMana);
            var newWidth = maxMana + 4;
            if (_manaBar.Width != newWidth)
                _manaBar.Width = newWidth;
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

            var mana = _scriptVars.Get<int>(Vars.PlayerMana);
            batcher.DrawRect(
                new Rectangle(
                    (HealthOffset + new Vector2(2, 3 + _fullHeart.SourceRect.Height + ElementSpacing)).ToPoint(),
                    new Point(mana, 4)),
                new Color(0xff935424));

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

            if (_saveIconAnimator.IsRunning)
            {
                var saveIcon = _saveIconAnimator.Sprite;
                var saveIconOffset = new Vector2(
                    Constants.ResWidth - saveIcon.SourceRect.Width,
                    Constants.ResHeight - saveIcon.SourceRect.Height) + SaveIconOffset;
                DrawSprite(batcher, saveIcon, saveIconOffset);
            }

            var val = _scriptVars.Get<string>(Vars.HudPrompt);
            var prompt = new FontCharacterSource(val);
            var size = _font.MeasureString(val);
            _font.DrawInto(batcher, ref prompt, new Vector2(Constants.ResWidth / 2, Constants.ResHeight - 20),
                Color.White, 0, size / 2, Vector2.One, SpriteEffects.None, 0);
        }

        void DrawSprite(Batcher batcher, Sprite sprite, Vector2 position, Vector2? scale = null)
        {
            batcher.Draw(sprite, position, Color.White, 0f,
                Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, 0);
        }
    }
}
