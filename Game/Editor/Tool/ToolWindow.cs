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
        Prefab,
        Select,
    }

    partial class ToolWindow : RenderableComponent, IUpdatable
    {
        public override float Width => 1;
        public override float Height => 1;

        public Tools CurrentTool { get; private set; } = Tools.Brush;

        RoomWindow _roomWindow;
        TilesetWindow _tilesetWindow;
        LayerWindow _layerWindow;
        Editor.Prefab.PrefabWindow _prefabWindow;
        EntityWindow _entityWindow;

        Brush _brush;
        Erase _erase;
        PrefabTool _prefab;
        SelectTool _select;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            _roomWindow = Entity.GetComponentStrict<RoomWindow>();
            _tilesetWindow = Entity.GetComponentStrict<TilesetWindow>();
            _layerWindow = Entity.GetComponentStrict<LayerWindow>();
            _prefabWindow = Entity.GetComponentStrict<Prefab.PrefabWindow>();
            _entityWindow = Entity.GetOrCreateComponent<EntityWindow>();

            _brush = new Brush(this);
            _erase = new Erase(this);
            _prefab = new PrefabTool(this);
            _select = new SelectTool(this);
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
            Entity.Position = Entity.Scene.Camera.MouseToWorldPoint();
            switch (CurrentTool)
            {
                case Tools.Brush: _brush.Update(); return;
                case Tools.Erase: _erase.Update(); return;
                case Tools.Prefab: _prefab.Update(); return;
                case Tools.Select: _select.Update(); return;
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            switch (CurrentTool)
            {
                case Tools.Prefab: _prefab.Render(batcher, camera); break;
            }
        }
    }
}
