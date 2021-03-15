using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    class VerticalLookAhead : Component, IUpdatable
    {
        public int LookAheadAmount = 64;
        public float LerpDuration = 0.2f;

        float _positionY = 0;

        VirtualIntegerAxis _input;
        VirtualButton _inputRangedModifier;

        float _timer;
        int _lastInput;

        public override void OnAddedToEntity()
        {
            _input = new VirtualIntegerAxis();
            _input.AddGamePadRightStickY();

            _inputRangedModifier = new VirtualButton();
            _inputRangedModifier.AddGamePadButton(0, Buttons.LeftShoulder);
            _inputRangedModifier.AddMouseRightButton();
        }

        public void Update()
        {
            _timer += Time.DeltaTime;

            var input = !_inputRangedModifier.IsDown ? _input.Value : 0;
            if (_lastInput != input)
                _timer = 0;

            _positionY = Mathf.Lerp(_positionY, -LookAheadAmount * input, _timer / LerpDuration);
            Entity.Scene.Camera.Position += new Vector2(0, (int)_positionY);

            _lastInput = input;
        }
    }
}
