using Nez;
using Game.Tiled;
using Game.Editor;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Tiled;
using Nez.Textures;
using Nez.Sprites;
using System.Linq;

namespace Game
{
    class EditorScene : Scene
    {
        const int LIGHT_LAYER = 1000;
        const int LIGHT_MAP_LAYER = 1001;

        public override void Initialize()
        {
            base.Initialize();

            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);
            //Screen.ApplyChanges();

            Time.TimeScale = 0;
            ClearColor = new Color(0xff371f0f);

            CreateEntity("windows")
                .AddComponent<PrefabWindow>();

            CreateEntity("controller")
                .AddComponent<EditorController>();

            var world = Json.ReadJson<World>("Content/Maps/world8.world");

            var layerRenderers = new Dictionary<int, RenderLayerRenderer>();
            foreach (var m in world.Maps)
            {
                var map = Content.LoadTiledMap($"Content/Maps/{m.FileName}");
                var terrainI = map.Layers.IndexOf(map.Layers.First(l => l.Name == "terrain"));
                var mapEntity = CreateEntity(Path.GetFileNameWithoutExtension(m.FileName));
                mapEntity.SetPosition(m.X, m.Y);
                for (var i = 0; i < map.Layers.Count; ++i)
                {
                    // set up render layer renderer
                    if (!layerRenderers.ContainsKey(i - terrainI))
                    {
                        layerRenderers[i - terrainI] = AddRenderer(new RenderLayerRenderer((i - terrainI) * 10, i - terrainI));
                    }

                    // set up tiled map renderer for tile layer
                    if (map.Layers[i] is TmxLayer tmxLayer)
                    {
                        var mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(map));
                        mapRenderer.SetLayerToRender(tmxLayer.Name);
                        mapRenderer.SetRenderLayer(i - terrainI);
                        if (tmxLayer.Name == "terrain")
                        {
                            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, Layer.Terrain);
                            mapRenderer.CollisionLayer = tmxLayer;
                        }
                    }
                }
            }
        }

        public override void OnStart()
        {
            // set up light renderer
            var lightRenderer = AddRenderer(new StencilLightRenderer(int.MinValue, LIGHT_LAYER, new RenderTexture()));
            lightRenderer.RenderTargetClearColor = new Color(127, 127, 127, 255);
            lightRenderer.CollidesWithLayers = 0;
            Flags.SetFlag(ref lightRenderer.CollidesWithLayers, Layer.Terrain);
            Flags.SetFlag(ref lightRenderer.CollidesWithLayers, Layer.Doodad);

            AddRenderer(new RenderLayerRenderer(-1, LIGHT_MAP_LAYER));

            CreateEntity("light-map")
                .AddComponent(new SpriteRenderer(lightRenderer.RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(LIGHT_MAP_LAYER)
                .Transform.SetParent(Camera.Transform);

            CreateEntity("light")
                .SetParent(Camera.Transform)
                .AddComponent(new StencilLight(64, Color.AntiqueWhite))
                .SetRenderLayer(LIGHT_LAYER);
        }
    }
}
