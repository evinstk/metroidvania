﻿using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    class TransportWindow : Component
    {
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
            var roomData = _roomWindow.RoomData;
            if (ImGui.Begin("Transport"))
            {
                if (ImGui.Button("Launch Room") && roomData != null)
                {
                    Core.Scene = new RoomScene(new RoomMetadata
                    {
                        RoomData = roomData,
                        Filename = _roomWindow.Filename,
                    });
                }
                ImGui.End();
            }
        }
    }
}