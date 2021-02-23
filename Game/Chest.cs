﻿using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;

namespace Game
{
    class Chest : Component, IInteractable, IUpdatable
    {
        public Sprite ClosedSprite;
        public Sprite OpenSprite;
        public ChestContents Contents = new ChestContents();
        public Collider Collider;

        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public void Interact(Entity interactor)
        {
            var inventory = Entity.Scene.GetPlayerInventory();
            foreach (var item in Contents.Items)
            {
                for (var i = 0; i < item.Quantity; ++i)
                    inventory.Add(item.Item);
            }
            Contents.Items.Clear();
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
