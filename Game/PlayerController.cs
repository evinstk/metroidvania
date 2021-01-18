using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    abstract class ControllerComponent : Component
    {
        public virtual float XAxis => 0;
        public virtual float YAxis => 0;
        public virtual bool JumpPressed => false;
        public virtual bool JumpDown => false;
        public virtual bool JumpReleased => false;
        public virtual bool AttackPressed => false;
        public virtual bool InteractPressed => false;
    }

    class PlayerController : ControllerComponent
    {
        VirtualAxis _xAxisInput;
        VirtualAxis _yAxisInput;
        VirtualButton _jumpInput;
        VirtualButton _attackInput;
        VirtualButton _interactInput;

        public PlayerController()
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

            _yAxisInput = new VirtualAxis();
            _yAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.W, Keys.S));
            _yAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(
                    VirtualInput.OverlapBehavior.CancelOut, Keys.Up, Keys.Down));
            _yAxisInput.Nodes.Add(
                new VirtualAxis.GamePadLeftStickY());

            _jumpInput = new VirtualButton();
            _jumpInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));
            _jumpInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));

            _attackInput = new VirtualButton();
            _attackInput.Nodes.Add(new VirtualButton.MouseLeftButton());
            _attackInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.X));

            _interactInput = new VirtualButton();
            _interactInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.S));
            _interactInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.DPadDown));
        }

        public override float XAxis => Enabled ? _xAxisInput.Value : 0;
        public override float YAxis => Enabled ? _yAxisInput.Value : 0;
        public override bool JumpDown => Enabled ? _jumpInput.IsDown : false;
        public override bool JumpPressed => Enabled ? _jumpInput.IsPressed : false;
        public override bool JumpReleased => Enabled ? _jumpInput.IsReleased : false;
        public override bool AttackPressed => Enabled ? _attackInput.IsPressed : false;
        public override bool InteractPressed => Enabled ? _interactInput.IsPressed : false;
    }
}
