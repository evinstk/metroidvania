﻿using Game.Editor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using Nez.Sprites;
using Nez.Textures;

namespace Game
{
    class RoomScene : Scene
    {
        string _roomDataId;

        public const int LIGHT_LAYER = 100;
        public const int LIGHT_MAP_LAYER = 101;

        public const int PHYSICS_TERRAIN = 1;

        public RoomScene(string roomDataId)
        {
            Insist.IsNotNull(roomDataId);
            _roomDataId = roomDataId;
        }

        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = new Color(0xff371f0f);

            AddRenderer(new RenderLayerExcludeRenderer(0, LIGHT_LAYER, LIGHT_MAP_LAYER));
        }

        public override void OnStart()
        {
            if (Core.GetGlobalManager<ImGuiManager>() != null)
            {
                CreateEntity("windows")
                    .AddComponent(new RoomTransport());
            }

            CreateEntity("roomLoader").AddComponent(new RoomLoader(_roomDataId));

            SetupLights();
        }

        void SetupLights()
        {
            var lightRenderer = AddRenderer(new StencilLightRenderer(-1, LIGHT_LAYER, new RenderTexture()));
            Flags.SetFlagExclusive(ref lightRenderer.CollidesWithLayers, PHYSICS_TERRAIN);
            var level = Core.GetGlobalManager<RoomManager>().GetResource(_roomDataId).LightRendererClearColor;
            lightRenderer.RenderTargetClearColor = new Color(level, level, level, 255);

            AddRenderer(new RenderLayerRenderer(1, LIGHT_MAP_LAYER));

            CreateEntity("light-map")
                .SetParent(Camera.Transform)
                .AddComponent(new SpriteRenderer(lightRenderer.RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(LIGHT_MAP_LAYER);
        }
    }

    class RoomTransport : Component
    {
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
            if (ImGui.Begin("Transport"))
            {
                if (ImGui.Button("Stop"))
                {
                    Core.Scene = new RoomEditorScene();
                }
                ImGui.End();
            }
        }
    }
}
