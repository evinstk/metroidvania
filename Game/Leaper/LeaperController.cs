using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;
using static Game.Editor.Prefab.PlayerMovementData;

namespace Game.Leaper
{
    class FreeLeaperController : Component, ILeaperController
    {
        public float XAxis { get; set; }
        public bool Leap { get; set; }
    }

    class AiLeaperControllerData : DataComponent
    {
        public int CastDistance = 125;
        public int JumpDistance = 75;
        public HitData HitData = new HitData();

        public override void AddToEntity(Entity entity)
        {
            var controller = entity.AddComponent<AiLeaperController>();
            controller.CastDistance = CastDistance;
            controller.JumpDistance = JumpDistance;
            controller.HitMask = HitData.HitMask;
        }
    }

    class AiLeaperController : Component, ILeaperController, IUpdatable
    {
        public int CastDistance = 125;
        public int JumpDistance = 75;
        public int HitMask = 0;

        public float XAxis { get; private set; } = 0f;
        public bool Leap { get; private set; } = false;

        static int[] _dirs = new int[] { -1, 1 };

        public void Update()
        {
            XAxis = 0f;
            Leap = false;

            var position = Entity.Position;
            var mask = HitMask;
            // cast for terrain layer too in case there's a wall between
            Flags.SetFlag(ref mask, PhysicsLayer.Terrain);

            foreach (var dir in _dirs)
            {
                var hit = Physics.Linecast(position, position + new Vector2(CastDistance * dir, 0), mask);
                if (hit.Collider != null && !Flags.IsUnshiftedFlagSet(hit.Collider.PhysicsLayer, PhysicsLayer.Terrain))
                {
                    XAxis = dir;
                    if (hit.Distance <= JumpDistance)
                    {
                        Leap = true;
                    }
                }
            }
        }
    }
}
