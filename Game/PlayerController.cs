using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    abstract class ControllerComponent : Component
    {
        public virtual float XAxis => 0;
        public virtual bool JumpPressed => false;
        public virtual bool JumpDown => false;
        public virtual bool AttackPressed => false;
    }

    class PlayerController : ControllerComponent
    {
        VirtualAxis _xAxisInput;
        VirtualButton _jumpInput;
        VirtualButton _attackInput;

        public override void OnAddedToEntity()
        {
            _xAxisInput = new VirtualAxis();
            _xAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D));
            _xAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right));
            _xAxisInput.Nodes.Add(
                new VirtualAxis.GamePadLeftStickX());

            _jumpInput = new VirtualButton();
            _jumpInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));
            _jumpInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));

            _attackInput = new VirtualButton();
            _attackInput.Nodes.Add(new VirtualButton.MouseLeftButton());
            _attackInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.X));
        }

        public override float XAxis => _xAxisInput.Value;
        public override bool JumpDown => _jumpInput.IsDown;
        public override bool JumpPressed => _jumpInput.IsPressed;
        public override bool AttackPressed => _attackInput.IsPressed;
    }
}
