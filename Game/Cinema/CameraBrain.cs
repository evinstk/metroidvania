using Microsoft.Xna.Framework;
using Nez;
using System;

namespace Game.Cinema
{
    class CameraBrain : Component, IUpdatable
    {
        public float TransitionDuration = 0.25f;

        VirtualCamera _lastCam;
        float _transitionTimer;
        Vector2 _transitionStart;

        public void Update()
        {
            var vcams = Entity.Scene.FindComponentsOfType<VirtualCamera>();

            if (vcams.Count > 0)
            {
                // take higher priority that's enabled
                vcams.Sort((a, b) =>
                {
                    var enabled = b.Enabled.CompareTo(a.Enabled);
                    return enabled == 0 ? b.Priority.CompareTo(a.Priority) : enabled;
                });
                var vcam = vcams[0];
                if (!vcam.Enabled) return;

                if (vcam != _lastCam && _lastCam != null)
                {
                    _transitionTimer = TransitionDuration;
                    _transitionStart = Entity.Position;
                }

                if (_transitionTimer > 0)
                {
                    _transitionTimer = Math.Max(_transitionTimer - Time.DeltaTime, 0);
                    Entity.Position = Vector2.Lerp(
                        _transitionStart,
                        vcam.CameraPosition,
                        (TransitionDuration - _transitionTimer) / TransitionDuration)
                        .ToPoint().ToVector2();
                }
                else
                {
                    Entity.Position = vcam.CameraPosition;
                }

                _lastCam = vcam;
            }
        }
    }
}
