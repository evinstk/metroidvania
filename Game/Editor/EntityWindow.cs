using Game.Editor.Prefab;
using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using System;
using System.Collections.Generic;
using static Game.Editor.Prefab.PrefabWindow;

namespace Game.Editor
{
    [EditorWindow]
    class EntityWindow : Component, IUpdatable
    {
        Type[] _componentSubclasses;
        List<EditorComponentInspector> _inspectors = new List<EditorComponentInspector>();
        List<EntityOnlyComponentInspector> _eoInspectors = new List<EntityOnlyComponentInspector>();

        public override void Initialize()
        {
            var subclasses = ReflectionUtils.GetAllSubclasses(typeof(DataComponent), true);
            subclasses.Sort((t, u) => t.Name.CompareTo(u.Name));
            _componentSubclasses = subclasses.ToArray();
        }

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
            Core.GetGlobalManager<RoomManager>().OnEntityOnlySync += GenerateInspectors;
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
            Core.GetGlobalManager<RoomManager>().OnEntityOnlySync -= GenerateInspectors;
        }

        void Draw()
        {
            if (EditorState.SelectedEntity == null) return;

            if (ImGui.Begin("Entity"))
            {
                DrawMain();
                ImGui.End();
            }
        }

        void DrawMain()
        {
            var entity = EditorState.SelectedEntity;
            ImGui.InputText("##name", ref entity.Name, 25);
            ImGui.SameLine();
            ImGui.Text("Name");

            var value = entity.Position.ToNumerics();
            if (ImGui.DragFloat2("##position", ref value))
            {
                entity.Position = value.ToXNA();
            }

            DataComponent toRemove = null;
            ImGui.Text("Components:");
            foreach (var group in _inspectors)
            {
                ImGui.PushID(group.Component.Id);
                var isHeaderOpen = ImGui.CollapsingHeader(group.Name);
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable("Remove Component"))
                    {
                        toRemove = group.Component;
                    }
                    ImGui.EndPopup();
                }
                if (isHeaderOpen)
                {
                    foreach (var inspector in group.Inspectors)
                    {
                        inspector.Draw();
                    }
                }
                ImGui.PopID();
            }

            foreach (var inspector in _eoInspectors)
            {
                inspector.Draw();
            }

            var addComponent = NezImGui.CenteredButton("Add Component", 1f);

            if (NezImGui.CenteredButton("Remove Entity", 1f))
            {
                EditorState.RoomData.Entities.Remove(entity);
                EditorState.SelectedEntityId = null;
            }

            if (addComponent)
            {
                ImGui.OpenPopup("component-selector");
            }
            if (toRemove != null)
            {
                entity.Components.Remove(toRemove);
                entity.SyncEntityOnlyComponents();
                GenerateInspectors();
            }

            DrawComponentSelectorPopup();
        }

        void DrawComponentSelectorPopup()
        {
            if (ImGui.BeginPopup("component-selector"))
            {
                foreach (var subclass in _componentSubclasses)
                {
                    if (ImGui.Selectable(subclass.Name))
                    {
                        var entity = EditorState.SelectedEntity;
                        entity.Components.Add(Activator.CreateInstance(subclass) as DataComponent);
                        entity.SyncEntityOnlyComponents();
                        GenerateInspectors();
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.EndPopup();
            }
        }

        void GenerateInspectors()
        {
            _inspectors = new List<EditorComponentInspector>();
            var data = EditorState.SelectedEntity;
            if (data == null) return;
            foreach (var component in data.Components)
            {
                _inspectors.Add(new EditorComponentInspector
                {
                    Name = component.GetType().Name,
                    Inspectors = TypeInspectorUtils.GetInspectableProperties(component),
                    //Id = NezImGui.GetScopeId(),
                    Component = component,
                });
            }

            _eoInspectors = new List<EntityOnlyComponentInspector>();
            foreach (var eoComponent in data.EntityOnlyComponents)
            {
                var inspector = new EntityOnlyComponentInspector(eoComponent);
                _eoInspectors.Add(inspector);
            }
        }

        string _lastEntityId = null;
        public void Update()
        {
            var selectedEntityId = EditorState.SelectedEntityId;
            if (_lastEntityId != selectedEntityId)
                GenerateInspectors();
            _lastEntityId = selectedEntityId;
        }
    }
}
