﻿using Nez;

namespace Game.Editor.Prefab
{
    class FollowCameraData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            var followCamera = entity.AddComponent(new FollowCamera(entity, FollowCamera.CameraStyle.CameraWindow));
            followCamera.FollowLerp = 1f;
        }
    }
}
