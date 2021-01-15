using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace Game
{
    class GateData : DataComponent
    {
        public TextureMapSpriteData ClosedSprite = new TextureMapSpriteData();
        public TextureMapSpriteData OpenSprite = new TextureMapSpriteData();

        public override void AddToEntity(Entity entity)
        {
            var gate = entity.AddComponent<Gate>();
            gate.ClosedSprite = ClosedSprite.Sprite;
            gate.OpenSprite = OpenSprite.Sprite;
        }
    }

    // TODO: make entity inspector attribute
    class GateState : DataComponent
    {
        public BooleanReference OpenState = new BooleanReference();

        public override void AddToEntity(Entity entity)
        {
            var gate = entity.GetComponent<Gate>();
            gate.OpenState = OpenState.Dereference();
        }
    }

    class Gate : Component, IUpdatable
    {
        public Sprite ClosedSprite;
        public Sprite OpenSprite;
        public BooleanValue OpenState;

        Collider _collider;
        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public void Update()
        {
            _collider.Enabled = !OpenState.Value;
            _renderer.Sprite = OpenState.Value ? OpenSprite : ClosedSprite;
        }
    }
}
