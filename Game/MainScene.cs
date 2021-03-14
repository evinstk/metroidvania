using Game.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Persistence;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    class MainScene : Scene
    {
        static readonly ScriptVars _vars = new ScriptVars(); // temp solution
        public ScriptVars ScriptVars => _vars;
        public Renderer FadeRenderer { get; private set; }

        Dictionary<string, World> _worlds = new Dictionary<string, World>();
        Dictionary<string, OgmoLevel> _levels = new Dictionary<string, OgmoLevel>();
        List<RoomBounds> _worldBounds = new List<RoomBounds>();
        List<RoomBounds> _runRooms = new List<RoomBounds>();
        RoomBounds _currentRoom;
        SceneScript _scripting;

        public readonly int SaveSlot;
        string _world;
        string _room;
        string _area;

        float _resetTimer = 0f;

        Vector2? _pendingMove = null;

        public MainScene(int saveSlot, string world, string room = null, string area = null)
        {
            SaveSlot = saveSlot;
            _world = world;
            _room = room;
            _area = area;
        }

        public override void Initialize()
        {
            SetDesignResolution(Constants.ResWidth, Constants.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(Constants.ScreenWidth, Constants.ScreenHeight);
            ClearColor = Constants.ClearColor;

            Physics.SpatialHashCellSize = 16;
            Physics.RaycastsStartInColliders = true;

            var lightRenderer = AddRenderer(new StencilLightRenderer(0, RenderLayers.Light, new RenderTexture()));
            lightRenderer.CollidesWithLayers = Mask.Terrain | Mask.Overlay;
            lightRenderer.RenderTargetClearColor = new Color(127, 127, 127, 255);

            FadeRenderer = AddRenderer(new RenderLayerRenderer(0, RenderLayers.Fade));
            FadeRenderer.RenderTexture = new RenderTexture();
            FadeRenderer.RenderTargetClearColor = Color.White;

            AddRenderer(new RenderLayerExcludeRenderer(1,
                RenderLayers.Dialog,
                RenderLayers.Light,
                RenderLayers.LightMap,
                RenderLayers.Hud,
                RenderLayers.PauseMenu,
                RenderLayers.PlayerMenu,
                RenderLayers.Fade,
                RenderLayers.FadeMap,
                RenderLayers.Null));
            AddRenderer(new RenderLayerRenderer(2, RenderLayers.LightMap, RenderLayers.FadeMap));
            AddRenderer(new ScreenSpaceRenderer(3, RenderLayers.PauseMenu, RenderLayers.PlayerMenu, RenderLayers.Dialog, RenderLayers.Hud));

            AddPostProcessor(new CinematicLetterboxPostProcessor(0));
        }

        public override void OnStart()
        {
            CreateEntity("light_map")
                .SetParent(Camera.Transform)
                .AddComponent(new SpriteRenderer(GetRenderer<StencilLightRenderer>().RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(RenderLayers.LightMap);

            Camera.AddComponent<CameraController>();
            Camera.AddComponent<CameraBounds>();
            Camera.Entity.UpdateOrder = int.MaxValue - 1;

            CreateEntity("fade")
                .SetParent(Camera.Transform)
                .AddComponent(new SpriteRenderer(FadeRenderer.RenderTexture))
                .SetMaterial(Material.BlendMultiply())
                .SetRenderLayer(RenderLayers.FadeMap);

            CreateEntity("sound_system").AddComponent<SoundSystem>();

            // TODO: load from save file
            ScriptVars.Set(Vars.PlayerInventory, new Inventory
            {
                Weapons = new List<Weapon> { (Weapon)Item.Get("Guard Baton") },
                EquippedWeaponIndex = 0,

                RangedWeapons = new List<RangedWeapon> { (RangedWeapon)Item.Get("Blaster") },
                EquippedRangedWeaponIndex = 0,
            });
            ScriptVars[Vars.PlayerMaxHealth] = 5;
            ScriptVars[Vars.PlayerHealth] = 5;

            CreateEntity("overlay").AddComponent<Overlay>();

            var dialogSystem = CreateEntity("dialog_system").AddComponent<DialogSystem>();
            dialogSystem.RenderLayer = RenderLayers.Dialog;

            var hud = CreateEntity("hud").AddComponent<Hud>();
            hud.RenderLayer = RenderLayers.Hud;

            CreateEntity("inventory_menu").AddComponent<InventoryMenu>();

            var pauseMenu = CreateEntity("pause_menu").AddComponent<PauseMenu>();
            pauseMenu.RenderLayer = RenderLayers.PauseMenu;

            _scripting = new SceneScript(dialogSystem, ScriptVars);

            var world = LoadWorld(_world);
            CreateEntity("world");
            AddWorldBounds(world);
            SetBackground(world);
            var startRoom = _room != null
                ? world.Rooms.Find(r => r.RoomName == _room)
                : world.Rooms.Find(r => r.Id == world.StartRoomId);
            Debug.LogIf(startRoom == null, "No start room set. Defaulting to room at (0, 0).");
            RunRoom(startRoom?.Position.ToVector2() ?? Vector2.Zero, _area);
        }

        World LoadWorld(string worldName)
        {
            if (_worlds.TryGetValue(worldName, out var world))
                return world;

            var worldPath = $"{ContentPath.Maps}{worldName}/world.json";
            var worldStr = File.ReadAllText(worldPath);
            world = Json.FromJson<World>(worldStr);
            world.Name = worldName;
            _worlds[worldName] = world;
            return world;
        }

        class RoomBounds
        {
            public OgmoLevel Level;
            public BoxCollider Collider;
            public Vector2 Position;
            public Room Room;
        }

        void AddWorldBounds(World world)
        {
            _worldBounds.Clear();
            var worldEntity = FindEntity("world");
            foreach (var room in world.Rooms)
            {
                var level = LoadLevel($"{world.Name}/{room.MapName}");
                var roomBounds = new RoomBounds
                {
                    Level = level,
                    Collider = new BoxCollider(
                        room.Position.X, room.Position.Y,
                        level.width, level.height),
                    Position = room.Position.ToVector2(),
                    Room = room,
                };
                roomBounds.Collider.PhysicsLayer = Mask.Room;
                roomBounds.Collider.Entity = worldEntity;
                Physics.AddCollider(roomBounds.Collider);
                //worldEntity.AddComponent(roomBounds.Collider);
                _worldBounds.Add(roomBounds);
            }
        }

        void SetBackground(World world)
        {
            var bgEntity = CreateEntity("background");
            bgEntity.Parent = Camera.Transform;
            for (var i = 0; i < world.Backgrounds.Count; ++i)
            {
                var background = world.Backgrounds[i];
                var renderer = bgEntity.AddComponent(new TiledSpriteRenderer(Content.LoadTexture($"{ContentPath.Backgrounds}{background.Filename}")));
                renderer.RenderLayer = RenderLayers.Background + i;
                var parallax = bgEntity.AddComponent(new TiledParallax());
                parallax.Renderer = renderer;
                parallax.ScrollScale = background.ScrollScale;
            }
        }

        OgmoLevel LoadLevel(string levelName)
        {
            if (_levels.TryGetValue(levelName, out var level))
                return level;

            var levelPath = $"{ContentPath.Maps}{levelName}";
            var levelStr = File.ReadAllText(levelPath);
            level = Json.FromJson<OgmoLevel>(levelStr);

            return level;
        }

        void RunRoom(Vector2 location, string startAreaName = null)
        {
            RoomBounds rb = null;
            foreach (var roomBounds in _worldBounds)
            {
                if (roomBounds.Collider.Bounds.Contains(location))
                    rb = roomBounds;
            }
            if (rb == null)
            {
                Debug.Log("No room found.");
                return;
            }

            Camera.GetComponent<CameraBounds>().Bounds = rb.Collider.Bounds;
            _currentRoom = rb;

            if (_runRooms.Contains(rb))
                return;

            var ogmoProjectStr = File.ReadAllText($"{ContentPath.Maps}Metroidvania.ogmo");
            var ogmoProject = Json.FromJson<OgmoProject>(ogmoProjectStr);
            var ogmoLevel = rb.Level;
            var map = CreateEntity("map");
            map.Position = rb.Position;
            for (var i = 0; i < ogmoLevel.layers.Count; ++i)
            {
                var layer = ogmoLevel.layers[i];
                if (layer.data != null)
                {
                    var isOverlay = layer.name == "overlay";
                    var renderer = map.AddComponent(new MapRenderer(ogmoProject, ogmoLevel, i));
                    switch (layer.name)
                    {
                        case "overlay":
                            renderer.RenderLayer = -50;
                            break;
                        case "above_details":
                            renderer.RenderLayer = -49;
                            break;
                        case "terrain":
                            renderer.RenderLayer = -48;
                            break;
                        default:
                            renderer.RenderLayer = i;
                            break;
                    }

                    if (layer.name == "terrain" || layer.name == "overlay")
                    {
                        var collider = map.AddComponent(new MapCollider(layer, ogmoLevel.width, ogmoLevel.height));
                        collider.PhysicsLayer = isOverlay ? Mask.Overlay : Mask.Terrain;
                    }
                }
                else if (layer.entities != null)
                {
                    foreach (var entity in layer.entities)
                    {
                        var pos = new Vector2(entity.x, entity.y) + rb.Position;
                        switch (entity.name)
                        {
                            // TODO: go with an attribute, reflection based approach
                            case "player":
                                if (FindComponentOfType<Player>() == null)
                                    this.CreatePlayer(pos);
                                break;
                            case "sentry":
                                this.CreateSentry(pos);
                                break;
                            case "switch":
                                this.CreateSwitch(pos, entity.values["state_var"]);
                                break;
                            case "door":
                                this.CreateDoor(pos, entity.values["state_var"]);
                                break;
                            case "chest":
                                this.CreateChest(pos, entity);
                                break;
                            case "cypher":
                                this.CreateCypher(pos);
                                break;
                            case "boss":
                                this.CreateBoss(pos);
                                break;
                            case "area":
                                this.CreateArea(pos, entity);
                                break;
                            case "stasis_chamber":
                                this.CreateStasisChamber(pos);
                                break;
                            case "dark_lord":
                                this.CreateDarkLord(pos);
                                break;
                            case "goblin":
                                this.CreateGoblin(pos);
                                break;
                            case "flat_door":
                                this.CreateFlatDoor(pos, entity);
                                break;
                            case "exit":
                                this.CreateExit(pos, entity);
                                break;
                            case "terminal":
                                this.CreateTerminal(pos, entity, _world, rb.Room.RoomName);
                                break;
                            default:
                                Debug.Log($"Unknown entity type {entity.name}");
                                break;
                        }
                    }
                }
            }

            if (startAreaName != null)
            {
                var area = FindEntity(startAreaName);
                if (area != null)
                {
                    var player = FindEntity("player");
                    if (player == null)
                        player = this.CreatePlayer(Vector2.Zero);
                    player.Position = area.Position;
                }
            }

            string script = null;
            if (ogmoLevel.values?.TryGetValue("script", out script) == true && script != "proj:")
                _scripting.LoadScript(Path.GetFileName(script));

            _runRooms.Add(rb);
        }

        public override void Update()
        {
            // restart
            if (Input.IsKeyDown(Keys.F2))
                Core.Scene = new MainScene(SaveSlot, _world, _room, _area);

            if (Timer.PauseTimer > 0)
            {
                Timer.PauseTimer -= Time.DeltaTime;
                if (Timer.PauseTimer <= -0.0001f)
                    Time.DeltaTime = -Timer.PauseTimer;
                else
                    return;
            }

            var player = FindEntity("player");
            if (player != null)
            {
                if (_pendingMove != null)
                {
                    player.Position = (Vector2)_pendingMove;
                    _pendingMove = null;
                }
                if (!_currentRoom.Collider.Bounds.Contains(player.Position))
                {
                    // TODO: turn this into smooth transition
                    RunRoom(player.Position);
                }
            }

            base.Update();

            _scripting.Update();

            var health = ScriptVars.Get<int>(Vars.PlayerHealth);
            if (health <= 0)
            {
                _resetTimer += Time.DeltaTime;
                if (Timer.OnTime(_resetTimer, 0.01f))
                {
                    FindComponentOfType<SoundSystem>().StopMusic(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }

                if (Timer.OnTime(_resetTimer, 2f))
                {
                    var gameOver = GameContent.LoadSound("Music", "game_over");
                    gameOver.start();

                    var transition = Core.StartSceneTransition(new FadeTransition(() =>
                    {
                        var save = Core.GetGlobalManager<SaveSystem>().Load(SaveSlot);
                        return save != null
                            ? new MainScene(SaveSlot, save.World, save.Room, save.Checkpoint)
                            : new MainScene(SaveSlot, "Intro");
                    }));
                    transition.FadeOutDuration = 0.3f;
                    transition.FadeInDuration = 0.2f;
                }
            }
        }

        public void MoveToArea(string room, string areaName)
        {
            foreach (var bounds in _worldBounds)
            {
                if (bounds.Room.RoomName != room)
                    continue;

                var entities = bounds.Level.layers.Find(l => l.name == "entities").entities;
                foreach (var entity in entities)
                {
                    var name = string.Empty;
                    if (entity.values?.TryGetValue("name", out name) == true && name == areaName)
                    {
                        _pendingMove = bounds.Position + new Vector2(entity.x, entity.y);
                        return;
                    }
                }
            }
            Debug.Log($"{areaName} not found.");
        }
    }

    static class MainSceneExt
    {
        public static MainScene GetMainScene(this Entity entity) => (MainScene)entity.Scene;
    }
}
