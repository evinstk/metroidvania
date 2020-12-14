using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game
{
    class SceneOptions
    {
        public int StartingHealth = -1;
        public bool UseLighting = false;
    }

	partial class MainScene : Scene
	{
        public string MapSrc { get; }
        public string Spawn { get; }
        public HashSet<Point> ClimbableTiles { get; }

        int _startingHealth;
        bool _useLighting;

        public const int ResWidth = 480;
        public const int ResHeight = 270;
        public const int ScreenWidth = 1920;
        public const int ScreenHeight = 1080;

        const int LIGHT_LAYER = -1;
        const int LIGHT_MAP_LAYER = -2;

        static Color _clearColor = new Color(5, 7, 14);

        public TmxMap Map { get; private set; }

        public MainScene(
            string mapSrc,
            string spawn,
            SceneOptions opts)
        {
            MapSrc = mapSrc;
            Spawn = spawn;
            _startingHealth = opts.StartingHealth;
            _useLighting = opts.UseLighting;

            SetDesignResolution(ResWidth, ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(ScreenWidth, ScreenHeight);

            ClearColor = _clearColor;

            Map = Content.LoadTiledMap("Content/Maps/" + MapSrc);

            for (var i = 0; i < Map.Layers.Count; ++i)
            {
                AddRenderer(new RenderLayerRenderer(i * 10, i));
            }

            var terrainLayer = Map.TileLayers.FirstOrDefault(l => l.Name == "terrain");
            if (terrainLayer != null)
            {
                ClimbableTiles = new HashSet<Point>(terrainLayer.Tiles.Where(t =>
                {
                    string climbable = string.Empty;
                    if (t?.TilesetTile?.Properties?.TryGetValue("climbable", out climbable) == true)
                    {
                        return bool.Parse(climbable);
                    }
                    return false;
                }).Select(t => new Point(t.X, t.Y)));
            }
            else
            {
                Debug.Log("No terrain layer found.");
                ClimbableTiles = new HashSet<Point>();
            }

            Physics.RaycastsHitTriggers = true;
        }

        public override void OnStart()
        {
            CreateEntity("gameManager").AddComponent<GameManager>();

            var mapEntity = CreateEntity("map");

            for (var i = 0; i < Map.Layers.Count; ++i)
            {
                var tileLayer = Map.Layers[i] as TmxLayer;
                if (tileLayer != null)
                {
                    var mapRenderer = mapEntity
                        .AddComponent(new TiledMapRenderer(Map));
                    mapRenderer.SetRenderLayer(i);
                    mapRenderer.SetLayersToRender(tileLayer.Name);
                    if (tileLayer.Name == "terrain")
                    {
                        Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, Layer.Terrain);
                        mapRenderer.CollisionLayer = tileLayer;
                    }
                }
            }

            var objects = Map.Layers.SelectMany((l, i) =>
            {
                var g = l as TmxObjectGroup;
                return g?.Objects.Select(o => (TmxObject: o, LayerIndex: i))
                    ?? Enumerable.Empty<(TmxObject TmxObject, int LayerIndex)>();
            });
            foreach ((var obj, var i) in objects)
            {
                if (obj.Tile != null)
                {
                    var tileset = Map.GetTilesetForTileGid(obj.Tile.Gid);
                    var sourceRect = tileset.TileRegions[obj.Tile.Gid];
                    var tileEntity = CreateEntity("parallax" + obj.Id.ToString())
                        .SetPosition(new Vector2(obj.X + sourceRect.Width / 2, obj.Y - sourceRect.Height / 2));
                    tileEntity.AddComponent(new SpriteRenderer(new Sprite(tileset.Image.Texture, sourceRect)))
                        .SetRenderLayer(i);

                    // parallax tiles
                    var layer = Map.Layers[i] as TmxObjectGroup;
                    string parallaxX = string.Empty;
                    if (layer.Properties?.TryGetValue("parallaxX", out parallaxX) == true)
                    {
                        tileEntity
                            .AddComponent<ParallaxComponent>()
                            .SetParallaxScale(new Vector2(float.Parse(parallaxX), 1));
                    }
                }

                if (obj.Type == "mobSpawn")
                {
                    var mobType = obj.Properties["mobType"];
                    if (!obj.Properties.TryGetValue("color", out var colorStr))
                        colorStr = "#ffffffff";
                    colorStr = "0x" + colorStr.Substring(1, 6) + "ff";
                    var color = new Color(Convert.ToUInt32(colorStr, 16));
                    color.A = 255;
                    if (!obj.Properties.TryGetValue("team", out var team))
                    {
                        team = "1";
                    }
                    obj.Properties.TryGetValue("dialog", out var dialogSrc);
                    if (dialogSrc != null) dialogSrc = Path.GetFileName(dialogSrc);
                    var mobEntity = Mob.MakeMobEntity(obj.Name != "" ? obj.Name : "mob", mobType, new MobOptions
                    {
                        Color = color,
                        Team = (Teams)int.Parse(team),
                        DialogSrc = dialogSrc,
                        RenderLayer = i,
                    });
                    mobEntity.Position = new Vector2(obj.X, obj.Y);
                }

                if (obj.Type == "trapdoor")
                {
                    MakeTrapdoor(obj, i);
                }

                if (obj.Type == "checkpoint")
                {
                    var width = 128; var height = 192;
                    CreateEntity(obj.Name != string.Empty ? obj.Name : "checkpoint" + obj.Id.ToString())
                        .SetPosition(new Vector2(obj.X, obj.Y))
                        .AddComponent(new RectangleRenderer(Color.PaleGoldenrod, width, height))
                        .AddComponent(new BoxCollider(0, 0))
                        .AddComponent<CheckpointComponent>();
                }

                // untyped objects are assumed to be triggers
                if ((obj.Type == "" || obj.Type == "exit") && obj.Tile == null)
                {
                    var triggerEntity = CreateEntity(obj.Name != string.Empty ? obj.Name : "trigger" + obj.Id.ToString());
                    var triggerCollider = triggerEntity.AddComponent(new BoxCollider(obj.X, obj.Y, obj.Width, obj.Height));
                    triggerCollider.IsTrigger = true;
                    Flags.SetFlagExclusive(ref triggerCollider.CollidesWithLayers, Layer.Default);
                    if (obj.Type == "exit")
                    {
                        var mapSrc = Path.GetFileName(obj.Properties["map"]);
                        var spawn = obj.Properties["spawn"];
                        triggerEntity.AddComponent(new ExitTrigger(mapSrc, spawn));
                    }
                }
            }

            var playerObj = objects.FirstOrDefault(x =>
                x.TmxObject.Name == Spawn && (x.TmxObject.Type == "playerSpawn" || x.TmxObject.Type == "checkpoint"));
            if (playerObj.TmxObject != null)
            {
                var playerEntity = Mob.MakeMobEntity("player", "Hero", new MobOptions
                {
                    PlayerControlled = true,
                    StartingHealth = _startingHealth,
                    Team = Teams.A,
                    RenderLayer = playerObj.LayerIndex,
                });
                playerEntity.Position = new Vector2(playerObj.TmxObject.X, playerObj.TmxObject.Y);

                var cameraEntity = Camera.Entity;
                var followCamera = playerEntity.AddComponent(new FollowCamera(playerEntity, Camera, FollowCamera.CameraStyle.CameraWindow));
                followCamera.MapLockEnabled = true;
                followCamera.MapSize = new Vector2(Map.TileWidth * Map.Width, Map.TileHeight * Map.Height);
                cameraEntity.Position = playerEntity.Position;
            }
            else
            {
                Debug.Log("No object with type \"playerSpawn\" found.");
            }

            string scriptSrc = null;
            if (Map.Properties?.TryGetValue("script", out scriptSrc) == true)
            {
                var mapScriptEntity = CreateEntity("mapScript");
                mapScriptEntity.AddComponent(new Scripting.MapScript(Path.GetFileName(scriptSrc)));
            }

            if (_useLighting) SetupLighting();
        }

        void SetupLighting()
        {
            var playerEntity = FindEntity("player");
            if (playerEntity == null)
            {
                Debug.Log("Cannot set up lighting without player entity.");
                return;
            }

            var lightRenderer = AddRenderer(new StencilLightRenderer(-1, LIGHT_LAYER, new RenderTexture()));
            lightRenderer.RenderTargetClearColor = new Color(127, 127, 127, 255);
            lightRenderer.CollidesWithLayers = 0;
            Flags.SetFlag(ref lightRenderer.CollidesWithLayers, Layer.Terrain);
            Flags.SetFlag(ref lightRenderer.CollidesWithLayers, Layer.Doodad);

            AddRenderer(new RenderLayerRenderer(
                playerEntity.GetComponent<RenderableComponent>().RenderLayer * 10 - 1,
                LIGHT_MAP_LAYER));

            CreateEntity("light-map")
                .AddComponent(new SpriteRenderer(lightRenderer.RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(LIGHT_MAP_LAYER)
                .Transform.SetParent(Camera.Transform);

            CreateEntity("light")
                .SetParent(playerEntity.Transform)
                .AddComponent(new StencilLight(400, Color.AntiqueWhite))
                .SetRenderLayer(LIGHT_LAYER);
        }

        static bool _transitioning = false;
        public SceneTransition Transition(string transitionSrc, string spawn)
        {
            if (_transitioning) return null;
            _transitioning = true;
            var playerHealth = FindEntity("player-hurtbox").GetComponent<HealthComponent>();
            var opts = new SceneOptions
            {
                StartingHealth = playerHealth.Health,
                UseLighting = _useLighting,
            };
            var nextScene = new MainScene(transitionSrc, spawn, opts);
            var transition = Core.StartSceneTransition(new FadeTransition(() => nextScene));
            transition.FadeOutDuration = 0.3f;
            transition.FadeInDuration = 0.2f;
            transition.OnTransitionCompleted += () => _transitioning = false;
            return transition;
        }

        public SceneTransition Reset()
        {
            if (_transitioning) return null;
            _transitioning = true;
            var scene = new MainScene(MapSrc, Spawn, new SceneOptions
            {
                UseLighting = _useLighting,
                StartingHealth = _startingHealth,
            });
            var transition = Core.StartSceneTransition(new FadeTransition(() => scene));
            transition.FadeOutDuration = 0.3f;
            transition.FadeInDuration = 0.2f;
            transition.OnTransitionCompleted += () => _transitioning = false;
            return transition;
        }
    }
}
