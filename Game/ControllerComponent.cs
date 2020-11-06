using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    class ControllerComponent : Component, IUpdatable
    {
        static int _speed = 300;

        Mover _mover;
        SubpixelVector2 _subpixelV2 = new SubpixelVector2();
        VirtualAxis _xAxisInput;

        public override void OnAddedToEntity()
        {
            _mover = Entity.AddComponent<Mover>();

            _xAxisInput = new VirtualAxis();
            _xAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D));
            _xAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right));
        }

        public void Update()
        {
            var moveDir = new Vector2(_xAxisInput.Value, 0);
            if (moveDir != Vector2.Zero)
            {
                var movement = moveDir * _speed * Time.DeltaTime;
                _mover.CalculateMovement(ref movement, out var res);
                _subpixelV2.Update(ref movement);
                _mover.ApplyMovement(movement);
            }
        }
    }
}
