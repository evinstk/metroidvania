using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.Persistence;
using System;
using System.Collections.Generic;

namespace Game.Editor
{
    public struct RoomVariable
    {
        public string Id;
        public string Name;
        public string Type;
        public object Value;

        public RoomVariable(RoomVariable var)
        {
            Id = var.Id;
            Name = var.Name;
            Type = var.Type;
            Value = var.Value;
        }
    }

    [EditorWindow]
    class RoomVariablesWindow : Component
    {
        static Dictionary<string, Type> _types = new Dictionary<string, Type>
        {
            { "Boolean", typeof(bool) },
            { "Int", typeof(int) },
            { "Float", typeof(float) },
            { "String", typeof(string) },
        };

        int _removalIndex = -1;

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
            var vars = EditorState.RoomData?.RoomVariables.Variables;
            if (vars == null) return;

            if (ImGui.Begin("Room Variables"))
            {
                for (var i = 0; i < vars.Count; ++i)
                {
                    ImGui.PushID(vars[i].Id);

                    var name = vars[i].Name;
                    if (ImGui.InputText("Name", ref name, 25))
                        vars[i] = new RoomVariable(vars[i]) { Name = name };

                    if (ImGui.BeginCombo("Type", vars[i].Type))
                    {
                        foreach (var t in _types)
                        {
                            if (ImGui.Selectable(t.Key, t.Key == vars[i].Type))
                            {
                                var newType = _types[t.Key];
                                vars[i] = new RoomVariable(vars[i])
                                {
                                    Type = t.Key,
                                    Value = newType != typeof(string) ? Activator.CreateInstance(newType) : string.Empty,
                                };
                            }
                        }
                        ImGui.EndCombo();
                    }

                    var type = _types[vars[i].Type];
                    if (type == typeof(bool))
                    {
                        var val = (bool)vars[i].Value;
                        if (ImGui.Checkbox("Value", ref val))
                            vars[i] = new RoomVariable(vars[i]) { Value = val };
                    }
                    else if (type == typeof(int))
                    {
                        var val = (int)vars[i].Value;
                        if (ImGui.InputInt("Value", ref val))
                            vars[i] = new RoomVariable(vars[i]) { Value = val };
                    }
                    else if (type == typeof(float))
                    {
                        var val = (float)vars[i].Value;
                        if (ImGui.InputFloat("Value", ref val))
                            vars[i] = new RoomVariable(vars[i]) { Value = val };
                    }
                    else if (type == typeof(string))
                    {
                        var val = (string)vars[i].Value;
                        if (ImGui.InputText("Value", ref val, 25))
                            vars[i] = new RoomVariable(vars[i]) { Value = val };
                    }
                    else
                    {
                        throw new Exception("Unsupported room variable type");
                    }

                    if (ImGui.Button("Remove Variable"))
                        _removalIndex = i;

                    ImGui.Separator();

                    ImGui.PopID();
                }

                if (_removalIndex > -1)
                    ImGui.OpenPopup("remove-variable");

                if (NezImGui.CenteredButton("Add Variable", 1f))
                {
                    vars.Add(new RoomVariable
                    {
                        Id = Utils.RandomString(),
                        Name = "New Variable",
                        Type = "Boolean",
                        Value = false,
                    });
                }

                if (ImGui.BeginPopupModal("remove-variable"))
                {
                    ImGui.Text($"Delete {vars[_removalIndex].Name}?");
                    if (ImGui.Button("Cancel"))
                    {
                        _removalIndex = -1;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Ok"))
                    {
                        vars.RemoveAt(_removalIndex);
                        _removalIndex = -1;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }

                ImGui.End();
            }
        }
    }
}
