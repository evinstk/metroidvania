using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;

namespace Game
{
    class Chest : Component, IUpdatable
    {
        public Sprite ClosedSprite;
        public Sprite OpenSprite;
        public Collider Collider;
        public string ContentsVar;

        SpriteRenderer _renderer;
        ScriptVars _vars;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
            _vars = Entity.Scene.GetScriptVars();
        }

        public void Update()
        {
            var openChests = _vars.Get<List<string>>(Vars.OpenChests);
            if (!openChests.Contains(Entity.Name))
            {
                _renderer.Sprite = ClosedSprite;
                Collider.PhysicsLayer |= Mask.Interaction;
            }
            else
            {
                _renderer.Sprite = OpenSprite;
                Collider.PhysicsLayer &= ~Mask.Interaction;
            }
        }
    }

    class ChestContents
    {
        public List<ChestItem> Items = new List<ChestItem>();
    }

    class ChestItem
    {
        public int Quantity;
        public Item Item;
    }
}
