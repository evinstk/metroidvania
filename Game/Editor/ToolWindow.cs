using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using System;
using System.IO;
using Num = System.Numerics;

namespace Game.Editor
{
    enum Tools
    {
        Brush,
        Erase,
        Select,
    }

    class ToolWindow : Component, IUpdatable
    {
        public Tools CurrentTool { get; private set; } = Tools.Brush;

        RoomWindow _roomWindow;
        TilesetWindow _tilesetWindow;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            _roomWindow = Entity.GetComponent<RoomWindow>();
            Insist.IsNotNull(_roomWindow);

            _tilesetWindow = Entity.GetComponent<TilesetWindow>();
            Insist.IsNotNull(_tilesetWindow);
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
                case Tools.Brush: UpdateBrush(); return;
                case Tools.Erase: UpdateErase(); return;
            }
        }

        void UpdateBrush()
        {
            var roomData = _roomWindow.RoomData;
            var textureFile = _tilesetWindow.TextureFile;

            if (Input.LeftMouseButtonDown
                && roomData != null
                && textureFile != null
                && Core.Scene.Camera.Bounds.Contains(Input.MousePosition))
            {
                var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                var layerPoint = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                if (layerPoint.X < 0 || layerPoint.X >= roomData.Width || layerPoint.Y < 0 || layerPoint.Y >= roomData.Height)
                    return;

                var tileSelection = _tilesetWindow.TileSelection;

                // TODO: select layer through window
                var layer = roomData.Layers[0];

                LayerTile existingTile = null;
                foreach (var tile in layer.Tiles)
                {
                    if (tile.LayerLocation == layerPoint)
                        existingTile = tile;
                }

                if (existingTile != null)
                    layer.Tiles.Remove(existingTile);

                layer.Tiles.Add(new LayerTile
                {
                    Tileset = Path.GetFileName(textureFile),
                    TilesetLocation = tileSelection,
                    LayerLocation = layerPoint,
                });
            }
        }

        void UpdateErase()
        {
            var roomData = _roomWindow.RoomData;

            if (Input.LeftMouseButtonDown
                && roomData != null
                && Core.Scene.Camera.Bounds.Contains(Input.MousePosition))
            {
                var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                var layerPoint = (worldPoint / roomData.TileSize.ToVector2()).ToPoint();

                if (layerPoint.X < 0 || layerPoint.X >= roomData.Width || layerPoint.Y < 0 || layerPoint.Y >= roomData.Height)
                    return;

                // TODO: select layer through window
                var layer = roomData.Layers[0];

                layer.Tiles.RemoveAll(t => t.LayerLocation == layerPoint);
            }
        }
    }
}
