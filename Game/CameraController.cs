using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class CameraController : Component, IUpdatable
    {
        Vector2? _focus = null;
        Vector2 _startPosition;
        Vector2 _lastPosition;
        float _panTimer;
        float _panDuration = 0.5f;

        public void Update()
        {
            if (_panTimer < _panDuration)
            {
                _panTimer += Time.DeltaTime;
            }

            if (_focus != null)
            {
                var lerpAmount = Mathf.Clamp01(_panTimer / _panDuration);
                var pos = Vector2.Lerp(_startPosition, (Vector2)_focus, lerpAmount);
                pos.X = (int)pos.X;
                pos.Y = (int)pos.Y;
                Entity.Position = pos;
            }

            _lastPosition = Entity.Position;
        }

        public void SetFocus(Vector2 focus, float duration = 1f)
        {
            _focus = focus;
            _panTimer = 0;
            _panDuration = duration;
            _startPosition = _lastPosition;
        }

        public void ReleaseFocus()
        {
            _focus = null;
        }
    }
}
