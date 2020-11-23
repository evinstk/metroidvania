using Nez;
using System;

namespace Game
{
    struct CheckpointPayload
    {
        public string MapSrc;
        public string Name;
    }

    class CheckpointComponent : Component, IUpdatable
    {
        public static Action<CheckpointPayload> OnCheckpoint;

        Collider _checkpointCollider;
        ControllerComponent _controller;
        Collider _playerCollider;

        public override void OnAddedToEntity()
        {
            _checkpointCollider = Entity.GetComponent<Collider>();

            var player = Entity.Scene.FindEntity("player");
            _controller = player.GetComponent<ControllerComponent>();
            _playerCollider = player.GetComponent<Collider>();
        }

        public void Update()
        {
            // TODO: fine for now, but what if multiple interact options?
            // don't want to save and talk to NPC on same button press
            if (_controller.InteractPressed && _checkpointCollider.CollidesWith(_playerCollider, out _))
            {
                OnCheckpoint(new CheckpointPayload
                {
                    MapSrc = (Entity.Scene as MainScene).MapSrc,
                    Name = Entity.Name,
                });
            }
        }
    }
}
