using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using System;
using System.IO;
using System.Linq;

namespace Game
{
    class SceneOptions
    {
        public int StartingHealth = -1;
        public bool UseLighting = false;
    }

	class MainScene : Scene
	{
        public string MapSrc { get; }
        public string Spawn { get; }

        int _startingHealth;
        bool _useLighting;

        const int resWidth = 1920;
        const int resHeight = 1080;
        const int screenWidth = 1920;
        const int screenHeight = 1080;

        const int LIGHT_LAYER = 5;
        const int LIGHT_MAP_LAYER = 10;
        const int BG_LAYER = 15;
        const int FG_LAYER = 20;

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
        }

        public override void Initialize()
        {
            SetDesignResolution(resWidth, resHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(screenWidth, screenHeight);

            AddRenderer(new RenderLayerRenderer(0, BG_LAYER));
            AddRenderer(new RenderLayerRenderer(1, LIGHT_MAP_LAYER));
            // default render layer
            AddRenderer(new RenderLayerRenderer(2, 0));
            AddRenderer(new RenderLayerRenderer(3, FG_LAYER));

            Physics.RaycastsHitTriggers = true;
        }

        public override void OnStart()
        {
            CreateEntity("gameManager").AddComponent<GameManager>();

            Map = Content.LoadTiledMap("Content/Maps/" + MapSrc);

            var mapEntity = CreateEntity("map");

            var mapRenderer = mapEntity.AddComponent(new TiledMapRenderer(Map, "terrain"));
            mapRenderer.SetLayersToRender(new[] { "background", "terrain" });
            Flags.SetFlagExclusive(ref mapRenderer.PhysicsLayer, 10);
            mapRenderer.SetRenderLayer(BG_LAYER);

            var fgLayer = Map.GetObjectGroup("foreground");
            if (fgLayer != null)
            {
                foreach (var obj in fgLayer.Objects)
                {
                    var tileset = fgLayer.Map.GetTilesetForTileGid(obj.Tile.Gid);
                    var sourceRect = tileset.TileRegions[obj.Tile.Gid];
                    CreateEntity("parallax" + obj.Id.ToString())
                        .SetPosition(new Vector2(obj.X + sourceRect.Width / 2, obj.Y - sourceRect.Height / 2))
                        .AddComponent(new SpriteRenderer(new Sprite(tileset.Image.Texture, sourceRect)))
                        .SetRenderLayer(FG_LAYER)
                        .AddComponent<ParallaxComponent>();
                }
            }

            var instanceLayer = Map.GetObjectGroup("instances");
            if (instanceLayer != null)
            {
                var instanceLookup = instanceLayer.Objects.ToLookup(t => t.Type);
                foreach (var mobSpawn in instanceLookup["mobSpawn"])
                {
                    var mobType = mobSpawn.Properties["mobType"];
                    if (!mobSpawn.Properties.TryGetValue("color", out var colorStr))
                        colorStr = "#ffffffff";
                    colorStr = "0x" + colorStr.Substring(1, 6) + "ff";
                    var color = new Color(Convert.ToUInt32(colorStr, 16));
                    color.A = 255;
                    if (!mobSpawn.Properties.TryGetValue("team", out var team))
                    {
                        team = "1";
                    }
                    mobSpawn.Properties.TryGetValue("dialog", out var dialogSrc);
                    if (dialogSrc != null) dialogSrc = Path.GetFileName(dialogSrc);
                    var mobEntity = Mob.MakeMobEntity(mobSpawn.Name != "" ? mobSpawn.Name : "mob", mobType, new MobOptions
                    {
                        Color = color,
                        Team = (Teams)int.Parse(team),
                        DialogSrc = dialogSrc,
                    });
                    mobEntity.Position = new Vector2(mobSpawn.X, mobSpawn.Y);
                }
                var width = 128; var height = 192;
                foreach (var checkpoint in instanceLookup["checkpoint"])
                {
                    var checkpointEntity = CreateEntity(checkpoint.Name != string.Empty ? checkpoint.Name : "checkpoint" + checkpoint.Id.ToString());
                    checkpointEntity.AddComponent(new RectangleRenderer(Color.PaleGoldenrod, width, height));
                    checkpointEntity.AddComponent(new BoxCollider(0, 0));
                    checkpointEntity.AddComponent<CheckpointComponent>();
                    checkpointEntity.Position = new Vector2(checkpoint.X, checkpoint.Y);
                }
            }

            var playerObj = instanceLayer.Objects.First(o =>
                o.Name == Spawn && (o.Type == "playerSpawn" || o.Type == "checkpoint"));
            var playerEntity = Mob.MakeMobEntity("player", "Hero", new MobOptions
            {
                PlayerControlled = true,
                StartingHealth = _startingHealth,
                Team = Teams.A,
            });
            playerEntity.Position = new Vector2(playerObj.X, playerObj.Y);

            var triggerLayer = Map.GetObjectGroup("triggers");
            if (triggerLayer != null)
            {
                foreach (var trigger in triggerLayer.Objects)
                {
                    var triggerEntity = CreateEntity(trigger.Name != string.Empty ? trigger.Name : "trigger" + trigger.Id.ToString());
                    var triggerCollider = triggerEntity.AddComponent(new BoxCollider(trigger.X, trigger.Y, trigger.Width, trigger.Height));
                    triggerCollider.IsTrigger = true;
                    Flags.SetFlagExclusive(ref triggerCollider.CollidesWithLayers, Layer.Default);
                    if (trigger.Type == "exit")
                    {
                        var mapSrc = Path.GetFileName(trigger.Properties["map"]);
                        var spawn = trigger.Properties["spawn"];
                        triggerEntity.AddComponent(new ExitTrigger(mapSrc, spawn));
                    }
                }
            }

            var cameraEntity = Camera.Entity;
            var followCamera = playerEntity.AddComponent(new FollowCamera(playerEntity, Camera, FollowCamera.CameraStyle.CameraWindow));
            followCamera.MapLockEnabled = true;
            followCamera.MapSize = new Vector2(Map.TileWidth * Map.Width, Map.TileHeight * Map.Height);
            cameraEntity.Position = playerEntity.Position;

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
            var lightRenderer = AddRenderer(new StencilLightRenderer(-1, LIGHT_LAYER, new RenderTexture()));
            lightRenderer.RenderTargetClearColor = new Color(127, 127, 127, 255);
            Flags.SetFlagExclusive(ref lightRenderer.CollidesWithLayers, 10);

            CreateEntity("light-map")
                .AddComponent<CenteringComponent>()
                .AddComponent(new SpriteRenderer(lightRenderer.RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(LIGHT_MAP_LAYER);

            CreateEntity("light")
                .SetParent(FindEntity("player").Transform)
                .AddComponent(new StencilLight(400, Color.AntiqueWhite))
                .SetRenderLayer(LIGHT_LAYER);
        }

        static bool _transitioning = false;
        public SceneTransition Transition(string transitionSrc, string spawn)
        {
            if (_transitioning) return null;
            _transitioning = true;
            var playerHealth = FindEntity("player").GetComponent<HealthComponent>();
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
            return Transition(MapSrc, Spawn);
        }
    }
}
