using Game.Editor;
using Game.Editor.Prefab;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using Nez.Persistence;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    class RoomScene : Scene
    {
        RoomMetadata _roomMetadata;

        const int LIGHT_LAYER = 100;
        const int LIGHT_MAP_LAYER = 101;

        public RoomScene(
            RoomMetadata roomMetadata)
        {
            Insist.IsNotNull(roomMetadata);
            _roomMetadata = roomMetadata;
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
                    .AddComponent(new RoomTransport(_roomMetadata));
            }

            var roomData = _roomMetadata.RoomData;

            var mapEntity = CreateEntity("map");
            var count = roomData.Layers.Count;
            for (var i = 0; i < count; ++i)
            {
                var layer = roomData.Layers[i];
                var layerEntity = CreateEntity(layer.Name);
                layerEntity.Transform.SetParent(mapEntity.Transform);
                layerEntity.AddComponent(new MapRenderer(roomData, i)).SetRenderLayer(count - 1 - i);

                if (layer.HasColliders)
                {
                    layerEntity.AddComponent(new MapCollider(roomData, i));
                }
            }

            // TODO: use entities folder in build folder
            var prefabs = new List<PrefabData>();
            var prefabsFolder = "../../../Content/Prefabs";
            foreach (var f in Directory.GetFiles(prefabsFolder))
            {
                var serializedEntity = File.ReadAllText(f);
                var entityData = Json.FromJson<PrefabData>(serializedEntity, new JsonSettings
                {
                    TypeConverters = Editor.Prefab.PrefabWindow.TypeConverters,
                });
                prefabs.Add(entityData);
            }
            foreach (var entityData in roomData.Entities)
            {
                var prefab = prefabs.Find(p => p.Id == entityData.PrefabId);
                Insist.IsNotNull(prefab);
                var entity = CreateEntity(entityData.Name);
                entity.SetPosition(entityData.Position);
                foreach (var component in prefab.Components)
                {
                    if (component is SpriteData spriteData)
                    {
                        var texture = spriteData.Texture;
                        var sprite = new Sprite(texture, spriteData.SourceRect, spriteData.Origin);
                        var renderer = entity.AddComponent(new SpriteRenderer(sprite));
                        renderer.Color = spriteData.Color;
                    }
                    if (component is LightData lightData)
                    {
                        CreateEntity("light")
                            .SetParent(entity.Transform)
                            .SetLocalPosition(lightData.LocalOffset)
                            .AddComponent(new StencilLight(lightData.Radius, lightData.Color, lightData.Power))
                            .SetRenderLayer(LIGHT_LAYER);
                    }
                }
            }

            SetupLights();
        }

        void SetupLights()
        {
            var lightRenderer = AddRenderer(new StencilLightRenderer(-1, LIGHT_LAYER, new RenderTexture()));
            var level = _roomMetadata.RoomData.LightRendererClearColor;
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
        RoomMetadata _roomMetadata;

        public RoomTransport(RoomMetadata roomMetadata)
        {
            _roomMetadata = roomMetadata;
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
                    Core.Scene = new RoomEditorScene(_roomMetadata);
                }
                ImGui.End();
            }
        }
    }
}
