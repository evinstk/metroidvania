using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;
using System.Collections.Generic;
using System.IO;
using Num = System.Numerics;

namespace Game.Editor.Prefab
{
    class EntityData
    {
        public string Name = "";
        public List<EditorComponent> Components = new List<EditorComponent>();
    }

    class EntityMetadata
    {
        public EntityData Data;
        public string Filename;
    }

    abstract class EditorComponent
    {
    }

    class PrefabWindow : Component
    {
        static string _prefabsFolder = "../../../Content/Prefabs";

        List<EntityMetadata> _entities = new List<EntityMetadata>();

        public EntityData SelectedEntity => _selectedEntity > -1 ? _entities[_selectedEntity].Data : null;
        int _selectedEntity = -1;

        public EntityData GetPrefabByName(string name)
        {
            var entity = _entities.Find(m => m.Data.Name == name);
            return entity.Data;
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
                var entityData = Json.FromJson<EntityData>(serializedEntity, new JsonSettings
                {
                    TypeConverters = _converters,
                });
                _entities.Add(new EntityMetadata
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
                });
            }
        }

        void DrawEntityInspector()
        {
            if (_selectedEntity == -1)
                return;

            var addComponent = false;

            var entity = _entities[_selectedEntity].Data;
            ImGui.Text("Name:");
            ImGui.InputText("##entityName", ref entity.Name, 25);

            ImGui.Text("Components:");
            foreach (var group in _inspectors)
            {
                if (ImGui.CollapsingHeader(group.Name))
                {
                    foreach (var inspector in group.Inspectors)
                    {
                        inspector.Draw();
                    }
                }
            }
            if (ImGui.Button("Add Component"))
                addComponent = true;

            if (addComponent)
            {
                entity.Components.Add(new SpriteData());
                GenerateInspectors();
            }
        }
    }
}
