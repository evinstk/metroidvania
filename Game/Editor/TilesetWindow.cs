using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ImGuiTools;
using System;
using System.IO;
using Num = System.Numerics;

namespace Game.Editor
{
    [EditorWindow]
    class TilesetWindow : Component
    {
        static readonly string TextureFolder = "../../../Content/Textures";

        Texture2D _texture;
        float _imageZoom = 3;
        Num.Vector2 _tileSize = new Num.Vector2(16, 16);

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
            if (EditorState.TilesetTextureFile != null)
                LoadTexture();
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
			ImGui.SetNextWindowPos(new Num.Vector2(1024, 25), ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowSize(new Num.Vector2(Screen.Width / 2, Screen.Height / 2), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Tileset", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();
                DrawTileset();
                ImGui.End();
            }
        }

        void DrawTileset()
        {
            if (_texturePtr != IntPtr.Zero)
            {
                var cursorPosImageTopLeft = ImGui.GetCursorScreenPos();
                var bounds = _texture.Bounds;
                ImGui.Image(_texturePtr, new Num.Vector2(bounds.Width, bounds.Height) * _imageZoom);

                DrawRect(cursorPosImageTopLeft + EditorState.TileSelection.ToNumerics() * _tileSize * _imageZoom);

                if (ImGui.IsWindowFocused() && ImGui.IsMouseDoubleClicked(0))
                {
                    var tilePos = (ImGui.GetMousePos() - cursorPosImageTopLeft) / (_tileSize * _imageZoom);
                    EditorState.TileSelection = tilePos.ToXNA().ToPoint();
                }
            }
        }

        void DrawRect(Num.Vector2 cursorPos)
        {
            ImGui.GetWindowDrawList().AddRect(
                cursorPos,
                cursorPos + _tileSize * _imageZoom,
                Color.GreenYellow.PackedValue);
        }

        void DrawMenuBar()
        {
            var openPng = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open PNG"))
                        openPng = true;
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (openPng)
                ImGui.OpenPopup("open-tileset-png");

            OpenTilesetPngPopup();
        }

        void OpenTilesetPngPopup()
        {
            if (ImGui.BeginPopupModal("open-tileset-png"))
            {
                var picker = FilePicker.GetFilePicker(this, TextureFolder);
                if (picker.Draw())
                {
                    EditorState.TilesetTextureFile = picker.SelectedFile;
                    LoadTexture();
                    FilePicker.RemoveFilePicker(this);
                }
                ImGui.EndPopup();
            }
        }

        IntPtr _texturePtr;
        void LoadTexture()
        {
            if (_texturePtr != IntPtr.Zero)
                Core.GetGlobalManager<ImGuiManager>().UnbindTexture(_texturePtr);

            _texture = Texture2D.FromStream(Core.GraphicsDevice, File.OpenRead(EditorState.TilesetTextureFile));
            _texturePtr = Core.GetGlobalManager<ImGuiManager>().BindTexture(_texture);
        }

        ~TilesetWindow()
        {
            if (_texturePtr != IntPtr.Zero)
                Core.GetGlobalManager<ImGuiManager>().UnbindTexture(_texturePtr);
        }
    }
}
