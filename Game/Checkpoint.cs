using Game.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    class Checkpoint : Component, IUpdatable
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

        public void Update()
        {
            // this logic should probably belong on a component on the player entity
            if (_input.IsPressed)
            {
                var rect = new RectangleF(
                    Entity.Position - Size / 2,
                    Size);
                var collider = Physics.OverlapRectangle(rect, PhysicsMask);
                if (collider != null)
                    SaveSystem2.Save(_roomEntity.RoomId, _roomEntity.RoomEntityId);
            }
        }
    }
}
