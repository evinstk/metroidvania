using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools.TypeInspectors;

namespace Game.Editor.Prefab
{
    class BoxColliderData : PrefabComponent
    {
        public Vector2 Size;
        //public int PhysicsLayer = -1;
        [CustomInspector(typeof(PhysicsInspector))]
        public class Physics
        {
            public int PhysicsLayer = -1;
            public int CollidesWithLayers = -1;
        }
        public Physics PhysicsData = new Physics();

        public override void AddToEntity(Entity entity)
        {
            var collider = entity.AddComponent<BoxCollider>();
            collider.SetSize(Size.X, Size.Y);
            collider.PhysicsLayer = PhysicsData.PhysicsLayer;
            collider.CollidesWithLayers = PhysicsData.CollidesWithLayers;
        }
    }

    class PhysicsInspector : AbstractTypeInspector
    {
        public override void DrawMutable()
        {
            var physicsData = GetValue<BoxColliderData.Physics>();
            ImGuiExt.DrawPhysicsLayerInput("Physics Layer", ref physicsData.PhysicsLayer);
            ImGuiExt.DrawPhysicsLayerInput("Collides With", ref physicsData.CollidesWithLayers);
        }
    }
}
