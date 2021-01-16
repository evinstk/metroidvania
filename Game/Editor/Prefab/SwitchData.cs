using Game.Editor.Scriptable;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Game.Editor.Prefab
{
    [EntityOnly(typeof(SwitchState))]
    class SwitchData : DataComponent
    {
        public TextureMapSpriteData OnSprite = new TextureMapSpriteData();
        public TextureMapSpriteData OffSprite = new TextureMapSpriteData();

        public override void AddToEntity(Entity entity)
        {
            var switchC = entity.AddComponent<Switch>();
            switchC.OnSprite = OnSprite.Sprite;
            switchC.OffSprite = OffSprite.Sprite;
        }
    }

    class SwitchState : EntityOnlyComponent
    {
        public BooleanReference State = new BooleanReference();

        public override void AddToEntity(Entity entity)
        {
            var switchC = entity.GetComponent<Switch>();
            switchC.State = State.Dereference();
        }
    }

    interface IInteractable
    {
        void Interact();
    }

    class Switch : Component, IUpdatable, IInteractable
    {
        public BooleanValue State = new BooleanValue();
        public Sprite OnSprite;
        public Sprite OffSprite;

        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public void Update()
        {
            _renderer.Sprite = State.Value ? OnSprite : OffSprite;
        }

        public void Interact()
        {
            State.Value = !State.Value;
        }
    }
}
