﻿using ImGuiNET;
using Microsoft.Xna.Framework.Input;
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

    [EditorWindow]
    partial class ToolWindow : RenderableComponent, IUpdatable
    {
        public override float Width => 1;
        public override float Height => 1;

        EntityWindow _entityWindow;

        Brush _brush;
        Erase _erase;
        PrefabTool _prefab;
        SelectTool _select;

        SubpixelVector2 _subpixelV2;

        public override void Initialize()
        {
            SetRenderLayer(-1);
        }

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

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
                    if (ImGui.RadioButton(i.ToString(), EditorState.CurrentTool == i))
                        EditorState.CurrentTool = i;
                }
                ImGui.End();
            }
        }

        public void Update()
        {
            Entity.Position = Entity.Scene.Camera.MouseToWorldPoint();
            if (Input.LeftMouseButtonDown && (Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt)))
            {
                Move();
            }
            else
            {
                switch (EditorState.CurrentTool)
                {
                    case Tools.Brush: _brush.Update(); return;
                    case Tools.Erase: _erase.Update(); return;
                    case Tools.Prefab: _prefab.Update(); return;
                    case Tools.Select: _select.Update(); return;
                }
            }
        }

        void Move()
        {
            var delta = Input.ScaledMousePositionDelta;
            _subpixelV2.Update(ref delta);
            Entity.Scene.Camera.Position -= delta;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            switch (EditorState.CurrentTool)
            {
                case Tools.Prefab: _prefab.Render(batcher, camera); break;
            }
        }
    }
}
