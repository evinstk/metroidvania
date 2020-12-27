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
        LayerWindow _layerWindow;

        Brush _brush;
        Erase _erase;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            _roomWindow = Entity.GetComponentStrict<RoomWindow>();
            _tilesetWindow = Entity.GetComponentStrict<TilesetWindow>();
            _layerWindow = Entity.GetComponentStrict<LayerWindow>();

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
            ImGui.SetNextWindowSize(new Num.Vector2(150, 100), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Tools"))
            {
                foreach (Tools i in Enum.GetValues(typeof(Tools)))
                {
                    if (ImGui.RadioButton(i.ToString(), CurrentTool == i))
                        CurrentTool = i;
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
