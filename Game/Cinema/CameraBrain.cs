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

        bool _pendingCut = false;
        public void QueueCut() => _pendingCut = true;

        public void Update()
        {
            _vcams.Clear();

            var activators = Entity.Scene.FindComponentsOfType<EntityVcamActivator>();
            activators.Sort((a, b) => b.Active.CompareTo(a.Active));
            if (activators.Count > 0 && activators[0].Active)
            {
                foreach (var activator in activators)
                {
                    if (!activator.Active) break;
                    var vcam = Entity.Scene.FindEntity(activator.VirtualCameraName)?.GetComponent<VirtualCamera>();
                    if (vcam != null)
                        _vcams.Add(vcam);
                    else
                        Debug.Log($"Virtual camera {activator.VirtualCameraName} not found.");
                }

                _vcams.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                if (_vcams.Count > 0 && _vcams[0] != null)
                {
                    var vcam = _vcams[0];
                    if (vcam != _lastCam && _lastCam != null)
                    {
                        if (!_pendingCut)
                        {
                            _transitionTimer = TransitionDuration;
                            _transitionStart = Entity.Position;
                        }
                        else
                        {
                            _pendingCut = false;
                        }
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
