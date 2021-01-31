using Nez;
using Game.Tiled;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Tiled;
using Nez.Textures;
using Nez.Sprites;
using System.Linq;

namespace Game.Editor
{
    class EditorScene : Scene
    {
        const int LIGHT_LAYER = 1000;
        const int LIGHT_MAP_LAYER = 1001;

        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

            Time.TimeScale = 0;
            ClearColor = new Color(0xff371f0f);

            CreateEntity("windows")
                .AddComponent<PrefabWindow>();

            CreateEntity("controller")
                .AddComponent<EditorController>();

            var world = IO.Json.ReadJson<Tiled.World>("Content/Maps/world16.world");

            var layerRenderers = new Dictionary<int, RenderLayerRenderer>();
            foreach (var m in world.Maps)
            {
                var map = Content.LoadTiledMap($"Content/Maps/{m.FileName}");
                var terrainI = map.Layers.IndexOf(map.Layers.First(l => l.Name == "terrain"));
                var mapEntity = CreateEntity(Path.GetFileNameWithoutExtension(m.FileName));
                mapEntity.SetPosition(m.X, m.Y);
                for (var i = 0; i < map.Layers.Count; ++i)
                {
                    var renderLayer = i - terrainI;

                    // set up render layer renderer
                    if (!layerRenderers.ContainsKey(renderLayer))
                    {
                        layerRenderers[renderLayer] = AddRenderer(new RenderLayerRenderer(renderLayer * 10, renderLayer));
                    }

                    // set up tiled map renderer for tile layer
                    if (map.Layers[i] is TmxLayer tmxLayer)
                    {
                        var mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(map));
                        mapRenderer.SetLayerToRender(tmxLayer.Name);
                        mapRenderer.SetRenderLayer(renderLayer);
                        if (tmxLayer.Name == "terrain")
                        {
                            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, Layer.Terrain);
                            mapRenderer.CollisionLayer = tmxLayer;
                        }
                    }

                    if (map.Layers[i] is TmxObjectGroup group)
                    {
                        string parallaxX = null;
                        group.Properties?.TryGetValue("parallaxX", out parallaxX);
                        string parallaxY = null;
                        group.Properties?.TryGetValue("parallaxY", out parallaxY);

                        foreach (var obj in group.Objects)
                        {
                            var entity = CreateEntity(obj.Name != string.Empty ? obj.Name : $"object-{obj.Id}");
                            entity.SetPosition(obj.X + m.X, obj.Y + m.Y);
                            if (obj.Tile != null)
                            {
                                entity.Position += new Vector2(obj.Tile.Tileset.TileWidth / 2, -obj.Tile.Tileset.TileHeight / 2);
                                var sourceRect = obj.Tile.Tileset.TileRegions[obj.Tile.Gid];
                                var sprite = new Sprite(obj.Tile.Tileset.Image.Texture, sourceRect);
                                entity.AddComponent(new SpriteRenderer(sprite))
                                    .SetRenderLayer(renderLayer);

                                var light = obj.Tile?.TilesetTile?.ObjectGroups?.SelectMany(o => o.Objects).FirstOrDefault(o => o.Type == "light");
                                if (light != null)
                                {
                                    var lightEntity = CreateEntity($"object-{obj.Id}-light");
                                    lightEntity.SetParent(entity.Transform);
                                    lightEntity.SetLocalPosition(new Vector2(light.X, light.Y) - new Vector2(obj.Tile.Tileset.TileWidth / 2, obj.Tile.Tileset.TileHeight / 2));
                                    lightEntity
                                        .AddComponent(new StencilLight(128, Color.AntiqueWhite))
                                        .SetRenderLayer(LIGHT_LAYER);
                                }
                            }
                            if (parallaxX != null || parallaxY != null)
                            {
                                entity
                                    .AddComponent<ParallaxComponent>()
                                    .SetParallaxScale(new Vector2(
                                        parallaxX != null ? float.Parse(parallaxX) : 0,
                                        parallaxY != null ? float.Parse(parallaxY) : 0));
                            }
                        }
                    }

                    if (map.Layers[i] is TmxImageLayer imageLayer)
                    {
                        string scrollX = null; string scrollY = null;
                        imageLayer.Properties?.TryGetValue("scrollX", out scrollX);
                        scrollX = scrollX ?? "1";
                        imageLayer.Properties?.TryGetValue("scrollY", out scrollY);
                        scrollY = scrollY ?? "0";

                        var imageEntity = CreateEntity(imageLayer.Name);
                        imageEntity.SetParent(Camera.Transform);
                        imageEntity
                            .AddComponent(new TiledSpriteRenderer(imageLayer.Image.Texture))
                            .SetRenderLayer(renderLayer)
                            .AddComponent<TiledParallaxComponent>()
                            .SetParallaxScale(new Vector2(
                                float.Parse(scrollX),
                                float.Parse(scrollY)));
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

            //CreateEntity("camera-light")
            //    .SetParent(Camera.Transform)
            //    .AddComponent(new StencilLight(64, Color.AntiqueWhite))
            //    .SetRenderLayer(LIGHT_LAYER);
        }
    }
}
