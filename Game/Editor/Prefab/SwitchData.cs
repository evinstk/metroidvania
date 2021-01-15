using Game.Editor.Scriptable;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Game.Editor.Prefab
{
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

    class SwitchState : DataComponent
    {
        public BooleanReference State = new BooleanReference();

        public override void AddToEntity(Entity entity)
        {
            var switchC = entity.GetComponent<Switch>();
            if (switchC != null)
            {
                var boolVal = State.Dereference();
                switchC.State = boolVal;
            }
            else
            {
                Debug.Log("No Switch component on entity.");
            }
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
