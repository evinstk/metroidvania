using Game.Editor;
using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    class CheckpointData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<Checkpoint>();
        }
    }

    class Checkpoint : Component, IInteractable
    {
        public Vector2 Size = new Vector2(32, 32);
        public int PhysicsMask = -1;

        VirtualButton _input;

        RoomEntityComponent _roomEntity;

        public override void OnAddedToEntity()
        {
            _input = new VirtualButton();
            _input.Nodes.Add(new VirtualButton.KeyboardKey(Keys.S));
            _input.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.DPadDown));

            _roomEntity = Entity.GetComponentStrict<RoomEntityComponent>();
        }

        public void Interact()
        {
            var scene = Entity.Scene as RoomScene;
            SaveSystem2.Save(scene.SaveSlotIndex, _roomEntity.WorldRoomId, _roomEntity.RoomEntityId);
        }
    }
}
