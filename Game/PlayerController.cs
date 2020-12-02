﻿using Microsoft.Xna.Framework.Input;
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
        InteractionComponent _interaction;

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

        public override void OnAddedToEntity()
        {
            _interaction = Entity.GetComponent<InteractionComponent>();
            Insist.IsNotNull(_interaction);
        }

        public override float XAxis => _interaction.InDialog ? 0 : _xAxisInput.Value;
        public override float YAxis => _interaction.InDialog ? 0 : _yAxisInput.Value;
        public override bool JumpDown => _interaction.InDialog ? false : _jumpInput.IsDown;
        public override bool JumpPressed => _interaction.InDialog ? false : _jumpInput.IsPressed;
        public override bool JumpReleased => _interaction.InDialog ? false : _jumpInput.IsReleased;
        public override bool AttackPressed => _interaction.InDialog ? false : _attackInput.IsPressed;
        public override bool InteractPressed => _interactInput.IsPressed;
    }
}
