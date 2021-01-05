using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using System.Collections.Generic;
using System.IO;
using Num = System.Numerics;

namespace Game.Editor
{
    [EditorWindow]
    class RoomWindow : Component
    {
        List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

        List<string> _scriptFilenames = new List<string>();
        public override void Initialize()
        {
            foreach (var f in Directory.GetFiles(ContentPath.Scripts, "*.lua"))
            {
                var filename = Path.GetFileName(f);
                if (filename != "common.lua")
                    _scriptFilenames.Add(filename);
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
            ImGui.SetNextWindowPos(new Num.Vector2(25, 500), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Num.Vector2(300, 300), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Room", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();
                DrawRoomData();

                ImGui.End();
            }
        }

        //string _newRoomName;
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
                Core.GetGlobalManager<RoomManager>().SaveAll();
        }

        void DrawRoomData()
        {
            var roomManager = Core.GetGlobalManager<RoomManager>();
            if (roomManager.Combo("Room", ref EditorState.SelectedRoomId))
            {
                var roomData = roomManager.GetResource(EditorState.SelectedRoomId);
                _inspectors = TypeInspectorUtils.GetInspectableProperties(roomData);
                Entity.Scene.Camera.Position = new Microsoft.Xna.Framework.Vector2(MainScene.ResWidth / 2, MainScene.ResHeight / 2);
            }

            if (NezImGui.CenteredButton("New Room", 1f))
            {
                var newRoomData = roomManager.NewResource();
                EditorState.SelectedRoomId = newRoomData.Id;
                _inspectors = TypeInspectorUtils.GetInspectableProperties(newRoomData);
                Entity.Scene.Camera.Position = new Microsoft.Xna.Framework.Vector2(MainScene.ResWidth / 2, MainScene.ResHeight / 2);
            }

            var room = EditorState.RoomData;
            if (room != null)
            {
                if (ImGui.BeginCombo("Script", room.Script))
                {
                    foreach (var filename in _scriptFilenames)
                    {
                        if (ImGui.Selectable(filename))
                            room.Script = filename;
                    }
                    ImGui.EndCombo();
                }

                foreach (var inspector in _inspectors)
                {
                    inspector.Draw();
                }
                if (ImGui.Button("Delete"))
                {
                    roomManager.Delete(EditorState.SelectedRoomId);
                    EditorState.SelectedRoomId = null;
                    _inspectors = new List<AbstractTypeInspector>();
                }
            }
        }
    }
}
