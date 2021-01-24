using Game.Editor;
using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Game.Scripting;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using Nez.Sprites;
using Nez.Textures;
using System;

namespace Game
{
    class RoomScene : Scene
    {
        public string RoomDataId { get; set; }
        public string CheckpointId { get; set; }

        public const int LIGHT_LAYER = 100;
        public const int LIGHT_MAP_LAYER = 101;
        public const int HUD_LAYER = 200;
        public const int UI_LAYER = 201;
        public const int PAUSE_MENU_LAYER = 202;

        public RoomScene(string roomDataId, string checkpointId = null)
        {
            Insist.IsNotNull(roomDataId);
            RoomDataId = roomDataId;
            CheckpointId = checkpointId;
        }

        public RoomScene()
        {
            var saveFile = SaveSystem2.Load();
            RoomDataId = saveFile.RoomId;
            Insist.IsNotNull(RoomDataId);
            CheckpointId = saveFile.CheckpointId;
            Insist.IsNotNull(CheckpointId);
        }

        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = new Color(0xff371f0f);
            Physics.SpatialHashCellSize = 16;
            Physics.RaycastsStartInColliders = true;

            AddRenderer(new RenderLayerExcludeRenderer(0, LIGHT_LAYER, LIGHT_MAP_LAYER, UI_LAYER, PAUSE_MENU_LAYER));
        }

        public override void OnStart()
        {
            SetupImGui();

            Camera.AddComponent<CameraBounds>();
            Camera.Entity.UpdateOrder = int.MaxValue - 1;

            CreateEntity("scripting").AddComponent<MapScript>();

            CreateEntity("roomLoader").AddComponent(new RoomLoader(RoomDataId, CheckpointId));

            SetupLights();
            SetupUi();
            SetupPauseMenu();

            var prefabManager = Core.GetGlobalManager<PrefabManager>();

            prefabManager
                .GetResource("OVMLPNUVMFKZTUTSTDEKNRKWLBDDCFXFIZABVQ")
                .CreateEntity("background", this);

            GC.Collect();
        }

        void SetupImGui()
        {
            if (Core.GetGlobalManager<ImGuiManager>() == null) return;

            CreateEntity("windows")
                .AddComponent(new RoomTransport(RoomDataId))
                .AddComponent<ScriptableObjectWindow>();
        }

        void SetupLights()
        {
            var lightRenderer = AddRenderer(new StencilLightRenderer(-1, LIGHT_LAYER, new RenderTexture()));
            Flags.SetFlagExclusive(ref lightRenderer.CollidesWithLayers, PhysicsLayer.Terrain);
            var level = Core.GetGlobalManager<RoomManager>().GetResource(RoomDataId).LightRendererClearColor;
            lightRenderer.RenderTargetClearColor = new Color(level, level, level, 255);

            AddRenderer(new RenderLayerRenderer(1, LIGHT_MAP_LAYER));

            CreateEntity("light-map")
                .SetParent(Camera.Transform)
                .AddComponent(new SpriteRenderer(lightRenderer.RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(LIGHT_MAP_LAYER);
        }

        void SetupUi()
        {
            AddRenderer(new ScreenSpaceRenderer(100, HUD_LAYER));
            Core.GetGlobalManager<PrefabManager>().GetResource("hud").CreateEntity("hud", this);

            AddRenderer(new ScreenSpaceRenderer(101, UI_LAYER));
            var dialogSystem = CreateEntity("dialogSystem");
            dialogSystem.SetParent(Camera.Transform);
            dialogSystem.AddComponent<DialogSystem>().SetRenderLayer(UI_LAYER);
        }

        void SetupPauseMenu()
        {
            AddRenderer(new ScreenSpaceRenderer(102, PAUSE_MENU_LAYER));
            var pauseMenu = CreateEntity("pause-menu").AddComponent<PauseMenu>();
            pauseMenu.RenderLayer = PAUSE_MENU_LAYER;
        }
    }

    static class PhysicsLayer
    {
        // layers 0-7 reserved for teams
        public const int Terrain = 8;
        public const int Interaction = 9;
    }

    class RoomTransport : Component
    {
        string _roomDataId;

        public RoomTransport(string roomDataId)
        {
            _roomDataId = roomDataId;
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
            if (ImGui.Begin("Transport"))
            {
                if (ImGui.Button("Stop"))
                {
                    Core.Scene = new RoomEditorScene();
                }
                ImGui.SameLine();
                if (ImGui.Button("Restart"))
                {
                    Core.Scene = new RoomScene(_roomDataId);
                }
                ImGui.End();
            }
        }
    }
}
