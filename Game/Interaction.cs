using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using System;
using System.Collections.Generic;

namespace Game
{
    class InteractionData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<Interaction>();
        }
    }

    class Interaction : Component, IUpdatable
    {
        public Vector2 Cast = new Vector2(32, 0);

        List<IInteractable> _tempInteractableList = new List<IInteractable>();
        int _facing = 0;
        Vector2 _lastPosition;

        VirtualButton _input;

        public override void OnAddedToEntity()
        {
            _input = new VirtualButton();
            _input.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));
            _input.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Y));

            _lastPosition = Entity.Position;
        }

        public void Update()
        {
            var diffX = Entity.Position.X - _lastPosition.X;
            if (diffX != 0)
                _facing = Math.Sign(diffX);
            _lastPosition = Entity.Position;

            if (_input.IsPressed)
            {
                var startInColliderDefault = Physics.RaycastsStartInColliders;
                Physics.RaycastsStartInColliders = true;
                var mask = 0;
                Flags.SetFlag(ref mask, PhysicsLayer.Interaction);
                var hit = Physics.Linecast(Entity.Position, Entity.Position + Cast * _facing, mask);
                Physics.RaycastsStartInColliders = startInColliderDefault;

                if (hit.Collider != null)
                {
                    hit.Collider.GetComponents(_tempInteractableList);
                    foreach (var interactable in _tempInteractableList)
                        interactable.Interact();
                    _tempInteractableList.Clear();
                }
            }
        }
    }
}
