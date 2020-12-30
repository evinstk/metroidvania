using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using Num = System.Numerics;

namespace Game.Editor.Prefab
{
    class PrefabData
    {
        public string Name = "";
        public List<PrefabComponent> Components = new List<PrefabComponent>();
    }

    class PrefabMetadata
    {
        public PrefabData Data;
        public string Filename;
    }

    abstract class PrefabComponent
    {
    }

    class PrefabWindow : Component
    {
        static string _prefabsFolder = "../../../Content/Prefabs";
        Type[] _componentSubclasses;

        List<PrefabMetadata> _entities = new List<PrefabMetadata>();

        public PrefabData SelectedEntity => _selectedEntity > -1 ? _entities[_selectedEntity].Data : null;
        int _selectedEntity = -1;

        public PrefabData GetPrefabByName(string name)
        {
            var entity = _entities.Find(m => m.Data.Name == name);
            return entity.Data;
        }

        public override void Initialize()
        {
            var subclasses = ReflectionUtils.GetAllSubclasses(typeof(PrefabComponent), true);
            subclasses.Sort((t, u) => t.Name.CompareTo(u.Name));
            _componentSubclasses = subclasses.ToArray();
        }

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            ReadEntities();
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        static JsonTypeConverter[] _converters = new JsonTypeConverter[]
        {
            new SpriteDataConverter(),
        };

        void ReadEntities()
        {
            foreach (var f in Directory.GetFiles(_prefabsFolder))
            {
                var serializedEntity = File.ReadAllText(f);
                var entityData = Json.FromJson<PrefabData>(serializedEntity, new JsonSettings
                {
                    TypeConverters = _converters,
                });
                _entities.Add(new PrefabMetadata
                {
                    Filename = f,
                    Data = entityData,
                });
            }
        }

        void SaveEntities()
        {
            foreach (var m in _entities)
            {
                var serializedEntity = Json.ToJson(m.Data, new JsonSettings
                {
                    PrettyPrint = true,
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = true,
                    TypeConverters = _converters,
                });
                File.WriteAllText(m.Filename, serializedEntity);
            }
        }

        void Draw()
        {
            ImGui.SetNextWindowSize(new Num.Vector2(Screen.Width / 2, Screen.Height / 2), ImGuiCond.FirstUseEver);

            var isOpen = true;
            if (ImGui.Begin("Entity", ref isOpen, ImGuiWindowFlags.MenuBar))
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
                SaveEntities();
        }

        class EditorComponentInspector
        {
            public string Name;
            public List<AbstractTypeInspector> Inspectors;
            public int Id;
            public PrefabComponent Component;
        }
        List<EditorComponentInspector> _inspectors;
        void DrawEntitySelector()
        {
            for (var i = 0; i < _entities.Count; ++i)
            {
                var data = _entities[i].Data;
                if (ImGui.RadioButton($"{i + 1}: {data.Name}", i == _selectedEntity))
                {
                    _selectedEntity = i;
                    GenerateInspectors();
                }
            }
        }

        void GenerateInspectors()
        {
            var data = _entities[_selectedEntity].Data;
            _inspectors = new List<EditorComponentInspector>();
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
            if (_selectedEntity == -1)
                return;

            var addComponent = false;
            PrefabComponent toRemove = null;

            var entity = _entities[_selectedEntity].Data;
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
                        _entities[_selectedEntity].Data.Components.Add(Activator.CreateInstance(subclass) as PrefabComponent);
                        GenerateInspectors();
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.EndPopup();
            }
        }
    }
}
