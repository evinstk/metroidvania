using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using System;
using Num = System.Numerics;

namespace Game.Editor.Tool
{
    enum Tools
    {
        Brush,
        Erase,
    }

    partial class ToolWindow : Component, IUpdatable
    {
        public Tools CurrentTool { get; private set; } = Tools.Brush;

        RoomWindow _roomWindow;
        TilesetWindow _tilesetWindow;

        Brush _brush;
        Erase _erase;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            _roomWindow = Entity.GetComponent<RoomWindow>();
            Insist.IsNotNull(_roomWindow);

            _tilesetWindow = Entity.GetComponent<TilesetWindow>();
            Insist.IsNotNull(_tilesetWindow);

            _brush = new Brush(this);
            _erase = new Erase(this);
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            ImGui.SetNextWindowPos(new Num.Vector2(25, 25), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Num.Vector2(300, 60), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Tools"))
            {
                if (ImGui.BeginCombo("Tool", CurrentTool.ToString()))
                {
                    foreach (int i in Enum.GetValues(typeof(Tools)))
                    {
                        if (ImGui.Selectable(((Tools)i).ToString()))
                            CurrentTool = (Tools)i;
                    }
                    ImGui.EndCombo();
                }
                ImGui.End();
            }
        }

        public void Update()
        {
            switch (CurrentTool)
            {
                case Tools.Brush: _brush.Update(); return;
                case Tools.Erase: _erase.Update(); return;
            }
        }
    }
}
