﻿using Nez;
using Nez.ImGuiTools.TypeInspectors;

namespace Game.Editor.Prefab
{
    [CustomInspector(typeof(PhysicsInspector))]
    class PhysicsLayerData
    {
        public int Mask = -1;
    }

    class PhysicsInspector : AbstractTypeInspector
    {
        public override void DrawMutable()
        {
            var physicsData = GetValue<PhysicsLayerData>();
            ImGuiExt.DrawPhysicsLayerInput(_name, ref physicsData.Mask);
        }
    }
}