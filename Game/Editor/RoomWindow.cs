using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using System.Collections.Generic;
using System.IO;
using Num = System.Numerics;

namespace Game.Editor
{
    class RoomWindow : Component
    {
        static string RoomFolder = "../../../Content/Rooms";

        public RoomData RoomData => _roomData;
        RoomData _roomData;
        public string Filename => _roomFile;
        string _roomFile;

        List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

        public RoomWindow(RoomMetadata roomMetadata)
        {
            _roomData = roomMetadata?.RoomData;
            _roomFile = roomMetadata?.Filename;
            if (roomMetadata != null)
                _inspectors = TypeInspectorUtils.GetInspectableProperties(_roomData);
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

        string _newRoomName;
        void DrawMenuBar()
        {
            var newRoom = false;
            var saveRoom = false;
            var openRoom = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Room"))
                        newRoom = true;
                    if (_roomFile != null && ImGui.MenuItem("Save"))
                        saveRoom = true;
                    if (ImGui.MenuItem("Open"))
                        openRoom = true;
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (newRoom)
                ImGui.OpenPopup("new-room");
            if (NewRoomPopup(out var newRoomName))
            {
                _newRoomName = newRoomName;
                ImGui.OpenPopup("new-room-file");
            }
            if (saveRoom)
                _roomData.SaveToFile(_roomFile);
            if (openRoom)
                ImGui.OpenPopup("open-room-file");

            NewRoomFilePopup();
            OpenRoomFilePopup();
        }

        string _newRoomNameInput = "";
        bool NewRoomPopup(out string newRoomName)
        {
            newRoomName = null;
            var isOpen = true;
            var newRoomFile = false;

            if (ImGui.BeginPopupModal("new-room", ref isOpen, ImGuiWindowFlags.NoTitleBar))
            {
                ImGui.Text("Room Name:");
                ImGui.InputText("##newRoomName", ref _newRoomNameInput, 25);

                if (ImGui.Button("Cancel"))
                {
                    _newRoomNameInput = "";
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Save") && _newRoomNameInput.Length > 0)
                {
                    newRoomName = _newRoomNameInput;
                    _newRoomNameInput = "";
                    newRoomFile = true;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            return newRoomFile;
        }

        void NewRoomFilePopup()
        {
            var isOpen = true;
            if (ImGui.BeginPopupModal("new-room-file", ref isOpen, ImGuiWindowFlags.NoTitleBar))
            {
                //var picker = FilePicker.GetFolderPicker(this, new DirectoryInfo(Environment.CurrentDirectory).Parent.FullName);
                var picker = FilePicker.GetFolderPicker(this, RoomFolder);
                picker.DontAllowTraverselBeyondRootFolder = false;
                if (picker.Draw())
                {
                    _roomFile = Path.Combine(picker.SelectedFile, _newRoomName + ".json");
                    _roomData = new RoomData
                    {
                        Name = _newRoomName,
                    };
                    _inspectors = TypeInspectorUtils.GetInspectableProperties(_roomData);
                    _roomData.SaveToFile(_roomFile);
                    FilePicker.RemoveFilePicker(this);
                }
                ImGui.EndPopup();
            }
        }

        void OpenRoomFilePopup()
        {
            var isOpen = true;
            if (ImGui.BeginPopupModal("open-room-file", ref isOpen, ImGuiWindowFlags.NoTitleBar))
            {
                var picker = FilePicker.GetFilePicker(this, RoomFolder);
                if (picker.Draw())
                {
                    _roomFile = picker.SelectedFile;
                    _roomData = RoomData.ReadFromFile(picker.SelectedFile);
                    _inspectors = TypeInspectorUtils.GetInspectableProperties(_roomData);
                    FilePicker.RemoveFilePicker(this);
                }
                ImGui.EndPopup();
            }
        }

        void DrawRoomData()
        {
            foreach (var inspector in _inspectors)
            {
                inspector.Draw();
            }
        }
    }
}
