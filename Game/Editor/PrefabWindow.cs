using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game.Editor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class PrefabAttribute : Attribute
    {
        public string Name;

        public PrefabAttribute(string name)
        {
            Name = name;
        }
    }

    class PrefabWindow : Component
    {
        List<MethodInfo> _methods;
        string _newEntityName = "";
        MethodInfo _popupMethod;

        public override void Initialize()
        {
            var assembly = typeof(PrefabWindow).GetTypeInfo().Assembly;
            _methods = assembly.DefinedTypes
                .SelectMany(t => t.DeclaredMethods)
                .Where(m => m.GetCustomAttributes(typeof(PrefabAttribute), false).Length > 0)
                .ToList();
            foreach (var method in _methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
                {
                    throw new Exception("Prefab factory must take one name parameter");
                }
                if (!method.IsStatic)
                {
                    throw new Exception("Prefab factory must be static");
                }
                if (method.ReturnType != typeof(Entity))
                {
                    throw new Exception("Prefab factory must be return Entity");
                }
            }
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
            ImGui.Begin("Prefabs");

            foreach (var method in _methods)
            {
                var prefab = method.GetCustomAttribute(typeof(PrefabAttribute)) as PrefabAttribute;
                if (NezImGui.CenteredButton(prefab.Name, 1f))
                {
                    _popupMethod = method;
                    ImGui.OpenPopup("create-prefab");
                }
            }

            DrawCreatePrefabPopup();

            ImGui.End();
        }

        void DrawCreatePrefabPopup()
        {
            if (ImGui.BeginPopup("create-prefab"))
            {
                ImGui.Text("New Entity Name:");
                ImGui.InputText("##newEntityName", ref _newEntityName, 25);

                if (ImGui.Button("Cancel"))
                {
                    _newEntityName = "";
                    ImGui.CloseCurrentPopup();
                }

				ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGui.GetItemRectSize().X);

				ImGui.PushStyleColor(ImGuiCol.Button, Color.Green.PackedValue);
                if (ImGui.Button("Create"))
                {
					_newEntityName = _newEntityName.Length > 0 ? _newEntityName : Utils.RandomString(8);
                    var newEntity = _popupMethod.Invoke(null, new[] { _newEntityName }) as Entity;
                    newEntity.Transform.Position = Core.Scene.Camera.Transform.Position;
                    Core.Scene.AddEntity(newEntity);

                    _newEntityName = "";
                    ImGui.CloseCurrentPopup();
                }
				ImGui.PopStyleColor();

                ImGui.EndPopup();
            }
        }
    }
}
