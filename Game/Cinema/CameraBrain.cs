using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;

namespace Game.Cinema
{
    class CameraBrain : Component, IUpdatable
    {
        public float TransitionDuration = 0.25f;

        VirtualCamera _lastCam;
        float _transitionTimer;
        Vector2 _transitionStart;

        List<VirtualCamera> _vcams = new List<VirtualCamera>();

        public void Update()
        {
            _vcams.Clear();

            var activators = Entity.Scene.FindComponentsOfType<PlayerActivator>();
            activators.Sort((a, b) => b.Active.CompareTo(a.Active));
            if (activators.Count > 0 && activators[0].Active)
            {
                foreach (var activator in activators)
                {
                    if (!activator.Active) break;
                    _vcams.Add(Entity.Scene.FindEntity(activator.VirtualCameraName)?.GetComponent<VirtualCamera>());
                }

                _vcams.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                if (_vcams.Count > 0 && _vcams[0] != null)
                {
                    var vcam = _vcams[0];
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
}
