﻿using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class PlatformerMover : Component, IUpdatable
    {
        public BoxCollider Collider;
        public Vector2 Speed;

        SubpixelFloat _movementRemainderX, _movementRemainderY;

        public void Update()
        {
            var motion = Speed * Time.DeltaTime;
            _movementRemainderX.Update(ref motion.X);
            _movementRemainderY.Update(ref motion.Y);

            var motionX = (int)motion.X;
            var motionY = (int)motion.Y;

            Entity.Position += new Vector2(motionX, motionY);
        }
    }
}
