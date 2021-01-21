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
        string _roomDataId;
        string _checkpointId;

        public const int LIGHT_LAYER = 100;
        public const int LIGHT_MAP_LAYER = 101;
        public const int UI_LAYER = 200;

        public RoomScene(string roomDataId)
        {
            Insist.IsNotNull(roomDataId);
            _roomDataId = roomDataId;
        }

        public RoomScene()
        {
            var saveFile = SaveSystem2.Load();
            _roomDataId = saveFile.RoomId;
            Insist.IsNotNull(_roomDataId);
            _checkpointId = saveFile.CheckpointId;
            Insist.IsNotNull(_checkpointId);
        }

        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = new Color(0xff371f0f);
            Physics.SpatialHashCellSize = 16;
            Physics.RaycastsStartInColliders = true;

            AddRenderer(new RenderLayerExcludeRenderer(0, LIGHT_LAYER, LIGHT_MAP_LAYER, UI_LAYER));
        }

        public override void OnStart()
        {
            SetupImGui();

            Camera.AddComponent<CameraBounds>();
            Camera.Entity.UpdateOrder = int.MaxValue - 1;

            CreateEntity("scripting").AddComponent<MapScript>();

            CreateEntity("roomLoader").AddComponent(new RoomLoader(_roomDataId, _checkpointId));

            SetupLights();
            SetupUi();

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
                .AddComponent(new RoomTransport(_roomDataId))
                .AddComponent<ScriptableObjectWindow>();
        }

        void SetupLights()
        {
            var lightRenderer = AddRenderer(new StencilLightRenderer(-1, LIGHT_LAYER, new RenderTexture()));
            Flags.SetFlagExclusive(ref lightRenderer.CollidesWithLayers, PhysicsLayer.Terrain);
            var level = Core.GetGlobalManager<RoomManager>().GetResource(_roomDataId).LightRendererClearColor;
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
            AddRenderer(new RenderLayerRenderer(100, UI_LAYER));
            var dialogSystem = CreateEntity("dialogSystem");
            dialogSystem.SetParent(Camera.Transform);
            dialogSystem.AddComponent<DialogSystem>().SetRenderLayer(UI_LAYER);
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
