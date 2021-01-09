using ImGuiNET;
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
        public List<DataComponent> Components = new List<DataComponent>();

        public T GetComponent<T>() where T : DataComponent => Components.Find(c => c is T) as T;
        public bool TryGetComponent<T>(out T component) where T : DataComponent
        {
            component = GetComponent<T>();
            return component != null;
        }

        public Entity CreateEntity(string name, Scene scene)
        {
            var entity = scene.CreateEntity(name);
            foreach (var component in Components)
            {
                component.AddToEntity(entity);
            }
            return entity;
        }
    }

    abstract class DataComponent
    {
        public virtual void AddToEntity(Entity entity) { }
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
            public int Id;
            public DataComponent Component;
        }
        List<EditorComponentInspector> _inspectors;
        void DrawEntitySelector()
        {
            bool newPrefab = false;

            if (_prefabManager.RadioButtons(EditorState.SelectedPrefabId, ref EditorState.SelectedPrefabId))
            {
                GenerateInspectors();
            }
            if (NezImGui.CenteredButton("New Prefab", 1f))
            {
                newPrefab = true;
            }

            if (newPrefab)
                ImGui.OpenPopup("new-resource");

            _prefabManager.NewResourcePopup();
        }

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
                    Id = NezImGui.GetScopeId(),
                    Component = component,
                });
            }
        }

        void DrawEntityInspector()
        {
            if (EditorState.SelectedPrefabId == null)
                return;

            var addComponent = false;
            DataComponent toRemove = null;

            var entity = _prefabManager.GetResource(EditorState.SelectedPrefabId);
            ImGui.Text("Name:");
            ImGui.InputText("##entityName", ref entity.Name, 25);

            ImGui.Text("Components:");
            foreach (var group in _inspectors)
            {
                ImGui.PushID(group.Id);
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
                addComponent = true;

            if (addComponent)
            {
                ImGui.OpenPopup("component-selector");
            }
            if (toRemove != null)
            {
                entity.Components.Remove(toRemove);
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
                        EditorState.SelectedPrefab.Components.Add(Activator.CreateInstance(subclass) as DataComponent);
                        GenerateInspectors();
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.EndPopup();
            }
        }
    }
}
