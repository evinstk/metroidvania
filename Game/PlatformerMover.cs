using Microsoft.Xna.Framework;
using Nez;
using System;

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

            MoveX(motionX);
            MoveY(motionY);
        }

        public bool OnGround(int dist = 1)
        {
            if (Collider == null)
                return false;

            return Check(new Vector2(0, dist));
        }

        bool Check(Vector2 dist)
        {
            var bounds = Collider.Bounds;
            bounds.Location += dist;
            return Physics.OverlapRectangle(bounds, Collider.CollidesWithLayers) != null;
        }

        void MoveX(int amount)
        {
            if (Collider != null)
            {
                int sign = Math.Sign(amount);
                while (amount != 0)
                {
                    if (Check(new Vector2(sign, 0)))
                    {
                        StopX();
                        return;
                    }
                    amount -= sign;
                    Entity.Position += new Vector2(sign, 0);
                }
            }
            else
            {
                Entity.Position += new Vector2(amount, 0);
            }
        }

        void StopX()
        {
            Speed.X = 0;
            _movementRemainderX.Reset();
        }

        void MoveY(int amount)
        {
            if (Collider != null)
            {
                int sign = Math.Sign(amount);
                while (amount != 0)
                {
                    if (Check(new Vector2(0, sign)))
                    {
                        StopY();
                        return;
                    }
                    amount -= sign;
                    Entity.Position += new Vector2(0, sign);
                }
            }
            else
            {
                Entity.Position += new Vector2(0, amount);
            }
        }

        void StopY()
        {
            Speed.Y = 0;
            _movementRemainderY.Reset();
        }
    }
}
