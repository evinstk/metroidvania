using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using System;
using System.Reflection;
using Num = System.Numerics;

namespace Game.Editor.Scriptable
{
    [EditorWindow]
    class ScriptableObjectWindow : Component
    {
        Type[] _soTypes;

        public override void Initialize()
        {
            var types = ReflectionUtils.GetAllSubclasses(typeof(ScriptableObject), true);
            types.Sort((t, u) => t.Name.CompareTo(u.Name));
            _soTypes = types.ToArray();
        }

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            if (ImGui.Begin("Scriptable Objects", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();


                ImGui.BeginChild("Selector", new Num.Vector2(200, 0), true);
                var newComponent = DrawSelector();
                ImGui.EndChild();

                ImGui.SameLine();

                ImGui.BeginChild("Inspector");
                DrawInspector();
                ImGui.EndChild();

                if (newComponent)
                    ImGui.OpenPopup("so-components");

                DrawScriptableObjectComponentsPopup();

                ImGui.End();
            }
        }

        bool DrawSelector()
        {
            foreach (var type in _soTypes)
            {
                Invoke(nameof(DrawSelectorForType), type);
            }
            var newComponent = NezImGui.CenteredButton("New Scriptable Object", 1f);

            return newComponent;
        }

        void DrawSelectorForType<T>()
            where T : ScriptableObject, new()
        {
            if (ImGui.CollapsingHeader(typeof(T).Name))
            {
                var manager = Core.GetGlobalManager<ScriptableObjectManager<T>>();
                var selectedId = EditorState.SelectedScriptableObject?.Id;
                if (manager.RadioButtons(ref selectedId))
                {
                    var so = manager.GetResource(selectedId);
                    EditorState.SelectedScriptableObject = so;
                }
            }
        }

        void DrawScriptableObjectComponentsPopup()
        {
            if (ImGui.BeginPopup("so-components"))
            {
                foreach (var type in _soTypes)
                    Invoke(nameof(Selectable), type);
                ImGui.EndPopup();
            }
        }

        void Invoke(string name, Type type, object[] parameters = null)
        {
            GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type).Invoke(this, parameters);
        }

        void Selectable<T>()
            where T : ScriptableObject, new()
        {
            if (ImGui.Selectable(typeof(T).Name))
            {
                var manager = Core.GetGlobalManager<ScriptableObjectManager<T>>();
                var obj = manager.NewResource();
                EditorState.SelectedScriptableObject = obj;
            }
        }

        void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save All"))
                    {
                        foreach (var type in _soTypes)
                            Invoke(nameof(SaveAll), type);
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
        }

        void SaveAll<T>()
            where T : ScriptableObject, new()
        {
            var manager = Core.GetGlobalManager<ScriptableObjectManager<T>>();
            manager.SaveAll();
        }

        void DrawInspector()
        {
            var inspectors = EditorState.SelectedScriptableObjectInspectors;
            if (inspectors == null) return;

            foreach (var inspector in inspectors)
            {
                inspector.Draw();
            }
        }
    }
}
