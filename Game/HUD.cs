using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Game
{
    class HUDData : DataComponent
    {
        public IntReference MaxHealth = new IntReference();
        public IntReference CurrHealth = new IntReference();

        public TextureMapSpriteData FullHeart = new TextureMapSpriteData();
        public TextureMapSpriteData EmptyHeart = new TextureMapSpriteData();

        public Vector2 HealthOffset = new Vector2(20, 20);
        public int HealthSpacing = 15;

        public override void AddToEntity(Entity entity)
        {
            var hud = entity.AddComponent<HUD>();

            hud.MaxHealth = MaxHealth.Dereference();
            hud.CurrHealth = CurrHealth.Dereference();

            hud.FullHeart = FullHeart.Sprite;
            hud.EmptyHeart = EmptyHeart.Sprite;

            hud.HealthOffset = HealthOffset;
            hud.HealthSpacing = HealthSpacing;
        }
    }

    class HUD : RenderableComponent
    {
        public override float Width => MainScene.ResWidth;
        public override float Height => MainScene.ResHeight;

        public IntValue MaxHealth;
        public IntValue CurrHealth;

        public Sprite FullHeart;
        public Sprite EmptyHeart;

        public Vector2 HealthOffset;
        public int HealthSpacing;

        public override void Initialize()
        {
            RenderLayer = RoomScene.HUD_LAYER;
        }

        public override void OnAddedToEntity()
        {
            CurrHealth.RuntimeValue = MaxHealth.RuntimeValue;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var maxHealth = MaxHealth.RuntimeValue;
            var currHealth = CurrHealth.RuntimeValue;
            for (var i = 0; i < maxHealth; ++i)
            {
                var sprite = i <= currHealth - 1 ? FullHeart : EmptyHeart;
                batcher.Draw(
                    sprite,
                    HealthOffset + new Vector2(HealthSpacing * i, 0),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1,
                    SpriteEffects.None,
                    0);
            }
        }
    }
}
