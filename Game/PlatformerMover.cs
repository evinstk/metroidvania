using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class PlatformerMover : Component
    {
        public class CollisionState
        {
            public bool Right, Left, Above, Below;
            public bool BecameGroundedThisFrame;
            public bool WasGroundedLastFrame;

            public bool HasCollision => Below || Right || Left || Above;

            internal SubpixelFloat _movementRemainderX, _movementRemainderY;

            public void Reset()
            {
                BecameGroundedThisFrame = Right = Left = Above = Below = false;
            }

            public void Reset(ref Vector2 motion)
            {
                if (motion.X == 0)
                    Right = Left = false;

                if (motion.Y == 0)
                    Above = Below = false;

                BecameGroundedThisFrame = false;

                // deal with subpixel movement, storing off any non-integar remainder for the next frame
                _movementRemainderX.Update(ref motion.X);
                _movementRemainderY.Update(ref motion.Y);

                if (Below && motion.Y == 0 && _movementRemainderY.Remainder > 0)
                {
                    motion.Y = 1;
                    _movementRemainderY.Reset();
                }
            }
        }

        public void Move(Vector2 motion, BoxCollider boxCollider, CollisionState collisionState)
        {
            TestCollisions(ref motion, boxCollider, collisionState);
            boxCollider.Entity.Transform.Position += motion;
        }

        public void TestCollisions(ref Vector2 motion, BoxCollider boxCollider, CollisionState collisionState)
        {
            collisionState.WasGroundedLastFrame = collisionState.Below;

            collisionState.Reset(ref motion);

            var motionX = (int)motion.X;
            var motionY = (int)motion.Y;

            // horizontal dir
            if (motionX != 0)
            {
                if (TestColliderCollision(boxCollider, new Vector2(motionX, 0), out var result))
                {
                    motion.X -= result.MinimumTranslationVector.X;
                    collisionState.Left = motionX < 0;
                    collisionState.Right = motionX > 0;
                    collisionState._movementRemainderX.Reset();
                }
                else
                {
                    collisionState.Left = false;
                    collisionState.Right = false;
                }
            }

            // vertical dir
            {
                if (TestColliderCollision(boxCollider, motion, out var result))
                {
                    motion.Y -= result.MinimumTranslationVector.Y;
                    collisionState.Above = motionY < 0;
                    collisionState.Below = motionY > 0;
                    collisionState._movementRemainderY.Reset();
                }
                else
                {
                    collisionState.Above = false;
                    collisionState.Below = false;
                }
            }

            if (!collisionState.WasGroundedLastFrame && collisionState.Below)
                collisionState.BecameGroundedThisFrame = true;
        }

        bool TestColliderCollision(BoxCollider boxCollider, Vector2 motion, out CollisionResult result)
        {
            var bounds = boxCollider.Bounds;
            bounds.Location += motion;

            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(boxCollider, ref bounds, boxCollider.CollidesWithLayers);
            foreach (var neighbor in neighbors)
            {
                if (neighbor.IsTrigger)
                    continue;

                if (boxCollider.CollidesWith(neighbor, motion, out result))
                    return true;
            }

            result = new CollisionResult();
            return false;
        }
    }
}
