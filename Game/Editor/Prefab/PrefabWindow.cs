using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using Num = System.Numerics;

namespace Game.Editor.Prefab
{
    class PrefabData : IResource
    {
        [JsonInclude]
        public string Id { get; set; } = Utils.RandomString();
        public string DisplayName => Name;
        public string Name = "New Prefab";
        public int UpdateOrder = 0;
        public List<DataComponent> Components = new List<DataComponent>();

        public List<PrefabData> ChildEntities = new List<PrefabData>();

        public T GetComponent<T>() where T : DataComponent => Components.Find(c => c is T) as T;
        public bool TryGetComponent<T>(out T component) where T : DataComponent
        {
            component = GetComponent<T>();
            return component != null;
        }

        public Entity CreateEntity(string name, Scene scene)
        {
            var entity = scene.CreateEntity(name);
            entity.UpdateOrder = UpdateOrder;
            foreach (var component in Components)
            {
                component.AddToEntity(entity);
            }
            foreach (var childEntityData in ChildEntities)
            {
                var childEntity = childEntityData.CreateEntity(childEntityData.Name, scene);
                childEntity.Parent = entity.Transform;
            }
            return entity;
        }

        public void Render(Batcher batcher, Vector2 position)
        {
            foreach (var component in Components)
                component.Render(batcher, position);
        }

        public bool Select(Vector2 entityPosition, Vector2 mousePosition)
        {
            foreach (var component in Components)
            {
                if (component.Select(entityPosition, mousePosition))
                    return true;
            }
            return false;
        }
    }

    abstract class DataComponent
    {
        [JsonInclude]
        public string Id { get; set; } = Utils.RandomString();

        public virtual void AddToEntity(Entity entity) { }
        public virtual void Render(Batcher batcher, Vector2 position) { }
        public virtual bool Select(Vector2 entityPosition, Vector2 mousePosition) => false;
    }

    [EditorWindow]
    class PrefabWindow : Component
    {
        Type[] _componentSubclasses;

        PrefabManager _prefabManager = Core.GetGlobalManager<PrefabManager>();

        public override void Initialize()
        {
            var subclasses = ReflectionUtils.GetAllSubclasses(typeof(DataComponent), true);
            subclasses.Sort((t, u) => t.Name.CompareTo(u.Name));
            _componentSubclasses = subclasses.ToArray();
        }

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            GenerateInspectors();
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            ImGui.SetNextWindowSize(new Num.Vector2(Screen.Width / 2, Screen.Height / 2), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Prefab", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();

                ImGui.BeginChild("Entities", new Num.Vector2(200, 0), true);
                DrawEntitySelector();
                ImGui.EndChild();

                ImGui.SameLine();

                ImGui.BeginChild("Inspector");
                DrawEntityInspector();
                ImGui.EndChild();

                ImGui.End();
            }
        }

        void DrawMenuBar()
        {
            var saveAll = false;
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save All"))
                        saveAll = true;
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (saveAll)
                _prefabManager.SaveAll();
        }

        public class EditorComponentInspector
        {
            public string Name;
            public List<AbstractTypeInspector> Inspectors;
            public DataComponent Component;
        }
        List<EditorComponentInspector> _inspectors;
        void DrawEntitySelector()
        {
            bool newPrefab = false;

            if (_prefabManager.RadioButtons(EditorState.SelectedPrefabId, ref EditorState.SelectedPrefabId, out var deletedId))
            {
                GenerateInspectors();
                _prefabManager.TriggerPrefabChange();
            }
            if (deletedId != null)
            {
                if (deletedId == EditorState.SelectedPrefabId)
                {
                    EditorState.SelectedPrefabId = null;
                    _inspectors = null;
                }
                // TODO: remove all room entities that use prefab
                _prefabManager.TriggerPrefabChange();
            }
            if (NezImGui.CenteredButton("New Prefab", 1f))
            {
                newPrefab = true;
            }

            if (newPrefab)
                ImGui.OpenPopup("new-resource");

            _prefabManager.NewResourcePopup();
        }

        class ChildInspector
        {
            public List<EditorComponentInspector> Inspectors = new List<EditorComponentInspector>();
            public PrefabData Entity;
        }
        List<ChildInspector> _childInspectors = new List<ChildInspector>();
        void GenerateInspectors()
        {
            _inspectors = new List<EditorComponentInspector>();
            var data = EditorState.SelectedPrefab;
            if (data == null) return;
            foreach (var component in data.Components)
            {
                _inspectors.Add(new EditorComponentInspector
                {
                    Name = component.GetType().Name,
                    Inspectors = TypeInspectorUtils.GetInspectableProperties(component),
                    Component = component,
                });
            }

            _childInspectors = new List<ChildInspector>();
            foreach (var childEntity in data.ChildEntities)
            {
                var childInspector = new ChildInspector
                {
                    Entity = childEntity,
                };
                foreach (var component in childEntity.Components)
                {
                    childInspector.Inspectors.Add(new EditorComponentInspector
                    {
                        Name = component.GetType().Name,
                        Inspectors = TypeInspectorUtils.GetInspectableProperties(component),
                        Component = component,
                    });
                }
                _childInspectors.Add(childInspector);
            }
        }

        void DrawEntityInspector()
        {
            if (EditorState.SelectedPrefabId == null)
                return;

            var addComponent = false;
            DataComponent toRemove = null;

            var entity = _prefabManager.GetResource(EditorState.SelectedPrefabId);
            ImGui.InputText("Name", ref entity.Name, 25);
            ImGui.InputInt("Update Order", ref entity.UpdateOrder);

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
            if (ImGui.Button("Add Component"))
            {
                ComponentListToAppend = entity.Components;
                addComponent = true;
            }

            PrefabData childToRemove = null;
            ImGui.Text("Entities:");
            foreach (var childInspector in _childInspectors)
            {
                DataComponent childCompToRemove = null;
                ImGui.PushID(childInspector.Entity.Id);

                ImGui.Indent();
                NezImGui.BeginBorderedGroup();

                ImGui.InputText("Name", ref childInspector.Entity.Name, 25);
                var isChildHeaderOpen = ImGui.CollapsingHeader(childInspector.Entity.Name);
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable("Remove Component"))
                        childToRemove = childInspector.Entity;
                    ImGui.EndPopup();
                }
                if (isChildHeaderOpen)
                {
                    foreach (var group in childInspector.Inspectors)
                    {
                        ImGui.PushID(group.Component.Id);
                        ImGui.Indent();
                        NezImGui.BeginBorderedGroup();
                        var isHeaderOpen = ImGui.CollapsingHeader(group.Name);
                        if (ImGui.BeginPopupContextItem())
                        {
                            if (ImGui.Selectable("Remove Component"))
                            {
                                childCompToRemove = group.Component;
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
                        NezImGui.EndBorderedGroup(new Num.Vector2(4, 1), new Num.Vector2(4, 2));
                        ImGui.Unindent();
                        ImGui.PopID();
                    }
                    if (ImGui.Button("Add Component"))
                    {
                        ComponentListToAppend = childInspector.Entity.Components;
                        addComponent = true;
                    }

                    if (childCompToRemove != null)
                    {
                        childInspector.Entity.Components.Remove(childCompToRemove);
                        GenerateInspectors();
                        _prefabManager.TriggerPrefabChange();
                    }
                }

                NezImGui.EndBorderedGroup(new Num.Vector2(4, 1), new Num.Vector2(4, 2));
                ImGui.Unindent();

                ImGui.PopID();
            }
            if (ImGui.Button("Add Child Entity"))
            {
                entity.ChildEntities.Add(new PrefabData());
                GenerateInspectors();
                _prefabManager.TriggerPrefabChange();
            }
            if (childToRemove != null)
            {
                entity.ChildEntities.Remove(childToRemove);
                GenerateInspectors();
                _prefabManager.TriggerPrefabChange();
            }

            if (addComponent)
            {
                ImGui.OpenPopup("component-selector");
            }
            if (toRemove != null)
            {
                entity.Components.Remove(toRemove);
                GenerateInspectors();
                _prefabManager.TriggerPrefabChange();
            }

            DrawComponentSelectorPopup();
        }

        List<DataComponent> ComponentListToAppend;
        void DrawComponentSelectorPopup()
        {
            if (ImGui.BeginPopup("component-selector"))
            {
                foreach (var subclass in _componentSubclasses)
                {
                    if (ImGui.Selectable(subclass.Name))
                    {
                        ComponentListToAppend.Add(Activator.CreateInstance(subclass) as DataComponent);
                        ComponentListToAppend = null;
                        GenerateInspectors();
                        _prefabManager.TriggerPrefabChange();
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.EndPopup();
            }
        }
    }
}
