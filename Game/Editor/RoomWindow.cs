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

        public RoomData RoomData => Core.GetGlobalManager<RoomManager>().GetResource(_selectedRoomId);
        string _selectedRoomId = null;

        List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

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

        //string _newRoomNameInput = "";
        //bool NewRoomPopup(out string newRoomName)
        //{
        //    newRoomName = null;
        //    var isOpen = true;
        //    var newRoomFile = false;

        //    if (ImGui.BeginPopupModal("new-room", ref isOpen, ImGuiWindowFlags.NoTitleBar))
        //    {
        //        ImGui.Text("Room Name:");
        //        ImGui.InputText("##newRoomName", ref _newRoomNameInput, 25);

        //        if (ImGui.Button("Cancel"))
        //        {
        //            _newRoomNameInput = "";
        //            ImGui.CloseCurrentPopup();
        //        }

        //        ImGui.SameLine();
        //        if (ImGui.Button("Save") && _newRoomNameInput.Length > 0)
        //        {
        //            newRoomName = _newRoomNameInput;
        //            _newRoomNameInput = "";
        //            newRoomFile = true;
        //            ImGui.CloseCurrentPopup();
        //        }

        //        ImGui.EndPopup();
        //    }

        //    return newRoomFile;
        //}

        //void NewRoomFilePopup()
        //{
        //    var isOpen = true;
        //    if (ImGui.BeginPopupModal("new-room-file", ref isOpen, ImGuiWindowFlags.NoTitleBar))
        //    {
        //        //var picker = FilePicker.GetFolderPicker(this, new DirectoryInfo(Environment.CurrentDirectory).Parent.FullName);
        //        var picker = FilePicker.GetFolderPicker(this, RoomFolder);
        //        picker.DontAllowTraverselBeyondRootFolder = false;
        //        if (picker.Draw())
        //        {
        //            _roomFile = Path.Combine(picker.SelectedFile, _newRoomName + ".json");
        //            _roomData = new RoomData
        //            {
        //                Name = _newRoomName,
        //            };
        //            _inspectors = TypeInspectorUtils.GetInspectableProperties(_roomData);
        //            _roomData.SaveToFile(_roomFile);
        //            FilePicker.RemoveFilePicker(this);
        //        }
        //        ImGui.EndPopup();
        //    }
        //}

        //void OpenRoomFilePopup()
        //{
        //    var isOpen = true;
        //    if (ImGui.BeginPopupModal("open-room-file", ref isOpen, ImGuiWindowFlags.NoTitleBar))
        //    {
        //        var picker = FilePicker.GetFilePicker(this, RoomFolder);
        //        if (picker.Draw())
        //        {
        //            _roomFile = picker.SelectedFile;
        //            _roomData = RoomData.ReadFromFile(picker.SelectedFile);
        //            _inspectors = TypeInspectorUtils.GetInspectableProperties(_roomData);
        //            FilePicker.RemoveFilePicker(this);
        //        }
        //        ImGui.EndPopup();
        //    }
        //}

        void DrawRoomData()
        {
            var roomManager = Core.GetGlobalManager<RoomManager>();
            if (roomManager.Combo("Room", _selectedRoomId, ref _selectedRoomId))
            {
                var roomData = roomManager.GetResource(_selectedRoomId);
                _inspectors = TypeInspectorUtils.GetInspectableProperties(roomData);
            }

            foreach (var inspector in _inspectors)
            {
                inspector.Draw();
            }
        }
    }
}
