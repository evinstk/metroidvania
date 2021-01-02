using Game.Editor;
using Game.Editor.Prefab;
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
        RoomData _roomData;

        public const int LIGHT_LAYER = 100;
        public const int LIGHT_MAP_LAYER = 101;

        public const int PHYSICS_TERRAIN = 1;

        public RoomScene(RoomData roomData)
        {
            Insist.IsNotNull(roomData);
            _roomData = roomData;
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

            var mapEntity = CreateEntity("map");
            var count = _roomData.Layers.Count;
            for (var i = 0; i < count; ++i)
            {
                var layer = _roomData.Layers[i];
                var layerEntity = CreateEntity(layer.Name);
                layerEntity.Transform.SetParent(mapEntity.Transform);
                layerEntity.AddComponent(new MapRenderer(_roomData, i)).SetRenderLayer(count - 1 - i);

                if (layer.HasColliders)
                {
                    var mapCollider = layerEntity.AddComponent(new MapCollider(_roomData, i));
                    Flags.SetFlagExclusive(ref mapCollider.PhysicsLayer, PHYSICS_TERRAIN);
                }
            }

            foreach (var entityData in _roomData.Entities)
            {
                var prefab = entityData.Prefab;
                Insist.IsNotNull(prefab);
                var entity = CreateEntity(entityData.Name);
                entity.SetPosition(entityData.Position);
                foreach (var component in prefab.Components)
                {
                    if (component is SpriteData spriteData)
                    {
                        var texture = spriteData.TextureData.Texture;
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
            Flags.SetFlagExclusive(ref lightRenderer.CollidesWithLayers, PHYSICS_TERRAIN);
            var level = _roomData.LightRendererClearColor;
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
