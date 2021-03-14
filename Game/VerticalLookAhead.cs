using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class VerticalLookAhead : Component, IUpdatable
    {
        public int LookAheadAmount = 64;
        public float LerpDuration = 0.2f;

        float _positionY = 0;

        VirtualIntegerAxis _input;
        float _timer;
        int _lastInput;

        public override void OnAddedToEntity()
        {
            _input = new VirtualIntegerAxis();
            _input.AddGamePadRightStickY();
        }

        public void Update()
        {
            //var lookAhead = _input.Value;
            //Entity.Scene.Camera.Position = Mathf.Lerp(
            //    Entity.Scene.Camera.Position,
            //    new Vector2(0, -lookAhead * LookAheadAmount),
            //    Time.DeltaTime);

            _timer += Time.DeltaTime;

            var input = _input.Value;
            if (_lastInput != input)
                _timer = 0;

            _positionY = Mathf.Lerp(_positionY, -LookAheadAmount * input, _timer / LerpDuration);
            Entity.Scene.Camera.Position += new Vector2(0, (int)_positionY);

            _lastInput = input;
        }
    }
}
