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
        public ChestContents Contents;

        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
            Contents = Entity.GetMainScene().GetScriptVars().Get<ChestContents>("baton_chest_contents");
        }

        public void Update()
        {
            if (Contents.Items.Count > 0)
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
