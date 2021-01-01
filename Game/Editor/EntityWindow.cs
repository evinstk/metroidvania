﻿using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    [EditorWindow]
    class EntityWindow : Component
    {
        public string Selection;

        RoomWindow _roomWindow;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
            _roomWindow = Entity.GetComponentStrict<RoomWindow>();
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            if (ImGui.Begin("Entity"))
            {
                var entity = _roomWindow.RoomData?.Entities.Find(e => e.Id == Selection);
                if (entity != null)
                {
                    if (NezImGui.CenteredButton("Remove Entity", 1f))
                    {
                        _roomWindow.RoomData.Entities.Remove(entity);
                    }

                    ImGui.InputText("##name", ref entity.Name, 25);
                    ImGui.SameLine();
                    ImGui.Text("Name");

                    var value = entity.Position.ToNumerics();
                    if (ImGui.DragFloat2("##position", ref value))
                    {
                        entity.Position = value.ToXNA();
                    }
                }
                ImGui.End();
            }
        }
    }
}