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
using System.Reflection;

namespace Game
{
    class RoomScene : Scene
    {
        public string WorldRoomId { get; set; } = "UMTEAJFYFJRRJIUSGVAWKPJINCWSEIPVGNMONX";
        public string CheckpointId { get; set; }
        public int SaveSlotIndex = -1;

        public const int LIGHT_LAYER = 100;
        public const int LIGHT_MAP_LAYER = 101;
        public const int PAUSE_MENU_LAYER = 200;
        public const int UI_LAYER = 201;
        public const int HUD_LAYER = 202;

        public RoomScene(string worldRoomId)
        {
            Insist.IsNotNull(worldRoomId);
            WorldRoomId = worldRoomId;
        }

        public RoomScene(int saveSlotIndex)
        {
            SaveSlotIndex = saveSlotIndex;
            if (SaveSystem2.Exists(saveSlotIndex))
            {
                var saveFile = SaveSystem2.Load(saveSlotIndex);
                WorldRoomId = saveFile.WorldRoomId;
                Insist.IsNotNull(WorldRoomId);
                CheckpointId = saveFile.CheckpointId;
                Insist.IsNotNull(CheckpointId);
                foreach (var saveSO in saveFile.ScriptableObjects)
                {
                    var manager = typeof(Core)
                        .GetMethod(nameof(Core.GetGlobalManager), BindingFlags.Public | BindingFlags.Static)
                        .MakeGenericMethod(typeof(ScriptableObjectManager<>).MakeGenericType(saveSO.GetType()))
                        .Invoke(null, null);
                    var so = manager.GetType().GetMethod("GetResource").Invoke(manager, new object[] { saveSO.Id });
                    // TODO: this will break if the interface is not implemented
                    so.GetType().GetProperty("RuntimeValue").SetValue(so, saveSO.GetType().GetProperty("RuntimeValue").GetValue(saveSO));
                }
            }
        }

        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = new Color(0xff371f0f);
            Physics.SpatialHashCellSize = 16;
            Physics.RaycastsStartInColliders = true;

            var soTypes = ReflectionUtils.GetAllSubclasses(typeof(ScriptableObject));
            foreach (var type in soTypes)
            {
                var manager = typeof(Core)
                    .GetMethod(nameof(Core.GetGlobalManager))
                    .MakeGenericMethod(typeof(ScriptableObjectManager<>).MakeGenericType(type))
                    .Invoke(null, null);
                manager.GetType().GetMethod("OnStart").Invoke(manager, null);
            }

            AddRenderer(new RenderLayerExcludeRenderer(0, LIGHT_LAYER, LIGHT_MAP_LAYER, HUD_LAYER, UI_LAYER, PAUSE_MENU_LAYER));
            AddRenderer(new ScreenSpaceRenderer(100, PAUSE_MENU_LAYER, UI_LAYER, HUD_LAYER));
        }

        public override void OnStart()
        {
            SetupImGui();

            Camera.AddComponent<CameraBounds>();
            Camera.Entity.UpdateOrder = int.MaxValue - 1;

            CreateEntity("scripting").AddComponent<MapScript>();

            CreateEntity("roomLoader").AddComponent(new RoomLoader(WorldRoomId, CheckpointId));

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
                .AddComponent(new RoomTransport(WorldRoomId))
                .AddComponent<ScriptableObjectWindow>();
        }

        void SetupLights()
        {
            var lightRenderer = AddRenderer(new StencilLightRenderer(-1, LIGHT_LAYER, new RenderTexture()));
            lightRenderer.CollidesWithLayers = 0;
            Flags.SetFlag(ref lightRenderer.CollidesWithLayers, PhysicsLayer.Terrain);
            Flags.SetFlag(ref lightRenderer.CollidesWithLayers, PhysicsLayer.Overlay);
            //var level = Core.GetGlobalManager<RoomManager>().GetResource(RoomDataId).LightRendererClearColor;
            lightRenderer.RenderTargetClearColor = new Color(127, 127, 127, 255);

            AddRenderer(new RenderLayerRenderer(1, LIGHT_MAP_LAYER));

            CreateEntity("light-map")
                .SetParent(Camera.Transform)
                .AddComponent(new SpriteRenderer(lightRenderer.RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(LIGHT_MAP_LAYER);
        }

        void SetupUi()
        {
            Core.GetGlobalManager<PrefabManager>().GetResource("hud").CreateEntity("hud", this);

            var dialogSystem = CreateEntity("dialogSystem");
            dialogSystem.AddComponent<DialogSystem>().SetRenderLayer(UI_LAYER);
        }

        void SetupPauseMenu()
        {
            var pauseMenu = CreateEntity("pause-menu").AddComponent<PauseMenu>();
            pauseMenu.RenderLayer = PAUSE_MENU_LAYER;
        }
    }

    static class PhysicsLayer
    {
        // layers 0-7 reserved for teams
        public const int Terrain = 8;
        public const int Interaction = 9;
        public const int Overlay = 10;
        public const int Platform = 11;
        public const int Room = 12;
    }

    class RoomTransport : Component
    {
        string _worldRoomId;

        public RoomTransport(string worldRoomId)
        {
            _worldRoomId = worldRoomId;
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
                    Core.Scene = new RoomScene(_worldRoomId);
                }
                ImGui.End();
            }
        }
    }
}
