using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;
using System;
using System.Collections.Generic;

namespace Game.Editor.Prefab
{
    class EntityOnlyAttribute : Attribute
    {
        public Type EntityOnlyComponentType;

        public EntityOnlyAttribute(Type type)
        {
            EntityOnlyComponentType = type;
        }
    }

    abstract class EntityOnlyComponent
    {
        [JsonInclude]
        [NotInspectable]
        public string DataComponentId { get; set; }

        public virtual void AddToEntity(Entity entity) { }
    }

    class EntityOnlyComponentInspector
    {
        string _name;
        List<AbstractTypeInspector> _inspectors;
        int _scopeId = NezImGui.GetScopeId();

        public EntityOnlyComponentInspector(EntityOnlyComponent component)
        {
            _name = $"{component.GetType().Name}";
            _inspectors = TypeInspectorUtils.GetInspectableProperties(component);
        }

        public void Draw()
        {
            ImGui.PushID(_scopeId);

            var isHeaderOpen = ImGui.CollapsingHeader(_name);
            if (isHeaderOpen)
            {
                foreach (var inspector in _inspectors)
                {
                    inspector.Draw();
                }
            }

            ImGui.PopID();
        }
    }
}
