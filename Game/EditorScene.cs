using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.ImGuiTools;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    public class EditorInit
    {
        public string World;
        public string RoomId;
    }

    class EditorScene : Scene
    {
        public string WorldName
        {
            get => _worldName;
            set
            {
                _worldName = value;
                OnWorldSet?.Invoke(this);
            }
        }
        string _worldName = string.Empty;
        public event Action<EditorScene> OnWorldSet;

        public World World => Worlds.ContainsKey(WorldName) ? Worlds[WorldName] : null;
        public Dictionary<string, World> Worlds = new Dictionary<string, World>();

        EditorInit _init;

        public EditorScene(EditorInit init = null)
        {
            _init = init;
        }

        public override void Initialize()
        {
            SetDesignResolution(Constants.ResWidth * 4, Constants.ResHeight * 4, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = new Color(0xff371f0f);

            foreach (var windowType in ReflectionUtils.GetAllTypesWithAttribute<EditorWindowAttribute>())
            {
                var entity = CreateEntity(windowType.Name);
                var window = Activator.CreateInstance(windowType) as Component;
                entity.AddComponent(window);
            }

            CreateEntity("world-renderer").AddComponent<WorldRenderer>();
            CreateEntity("editor-controller").AddComponent<EditorController>();
        }

        public override void OnStart()
        {
            if (_init != null)
            {
                var worldWindow = FindComponentOfType<WorldWindow>();
                worldWindow.SetWorld(_init.World, _init.RoomId);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class EditorWindowAttribute : Attribute
    {
    }

    [EditorWindow]
    class WorldWindow : Component
    {
        List<string> _worlds;
        string _launchRoomId;
        int _saveSlot;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            _worlds = new List<string>();
            var dirInfo = new DirectoryInfo(ContentPath.Maps);
            foreach (var worldDir in dirInfo.GetDirectories())
                _worlds.Add(worldDir.Name);
        }

        public override void OnRemovedFromEntity()
        {
            _worlds = null;
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            var scene = Entity.Scene as EditorScene;
            if (ImGui.Begin("World", ImGuiWindowFlags.MenuBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Save All"))
                        {
                            foreach (var kv in scene.Worlds)
                            {
                                var path = ContentPath.Maps + kv.Key + "/world.json";
                                var json = Json.ToJson(kv.Value, true);
                                File.WriteAllText(path, json);
                            }
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }

                if (ImGui.BeginCombo("World", scene.WorldName))
                {
                    foreach (var world in _worlds)
                    {
                        if (ImGui.Selectable(world, world == scene.WorldName))
                            SetWorld(world);
                    }
                    ImGui.EndCombo();
                }

                var currWorld = scene.World;
                if (currWorld != null)
                {
                    ImGui.Separator();

                    var launchRoom = currWorld.Rooms.Find(r => r.Id == _launchRoomId);
                    if (ImGui.BeginCombo("##launch-room", launchRoom?.MapName))
                    {
                        foreach (var room in currWorld.Rooms)
                        {
                            if (ImGui.Selectable(room.RoomName))
                                _launchRoomId = room.Id;
                        }
                        ImGui.EndCombo();
                    }

                    if (ImGui.Button("Launch") && _launchRoomId != null)
                    {
                        var save = Core.GetGlobalManager<SaveSystem>().Load(_saveSlot);
                        Core.Scene = new MainScene(_saveSlot, save, new StartRoom
                        {
                            World = scene.WorldName,
                            RoomId = _launchRoomId,
                        });
                    }
                }

                ImGui.End();
            }
        }

        public void SetWorld(string worldName, string launchRoom = null)
        {
            var scene = Entity.Scene as EditorScene;
            scene.WorldName = worldName;
            if (!scene.Worlds.ContainsKey(worldName))
            {
                World world;
                try
                {
                    var worldStr = File.ReadAllText(ContentPath.Maps + worldName + "/world.json");
                    world = Json.FromJson<World>(worldStr);
                }
                catch (FileNotFoundException)
                {
                    world = new World();
                }
                scene.Worlds.Add(worldName, world);
            }
            _launchRoomId = launchRoom;
        }
    }

    [EditorWindow]
    class MapWindow : Component, IUpdatable
    {
        World _lastWorld;
        List<string> _rooms = new List<string>();
        string _room = string.Empty;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        public void Update()
        {
            var scene = Entity.Scene as EditorScene;
            var currWorld = scene.World;

            if (currWorld != _lastWorld)
            {
                _rooms.Clear();
                if (currWorld != null)
                {
                    var dirInfo = new DirectoryInfo(ContentPath.Maps + scene.WorldName);
                    foreach (var map in dirInfo.GetFiles("*.json"))
                    {
                        if (map.Name != "world.json")
                            _rooms.Add(map.Name);
                    }
                }
            }

            _lastWorld = currWorld;
        }

        void Draw()
        {
            var scene = Entity.Scene as EditorScene;
            var world = scene.World;
            if (world == null) return;

            if (ImGui.Begin("Rooms"))
            {
                if (ImGui.BeginCombo("Start Room", world.Rooms.Find(r => world.StartRoomId == r.Id)?.RoomName))
                {
                    foreach (var room in world.Rooms)
                    {
                        if (ImGui.Selectable(room.RoomName, room.Id == world.StartRoomId))
                        {
                            world.StartRoomId = room.Id;
                        }
                    }
                    ImGui.EndCombo();
                }
                ImGui.Separator();

                Room toRemove = null;
                foreach (var room in world.Rooms)
                {
                    ImGui.PushID(room.Id);

                    ImGui.InputText("Room Name", ref room.RoomName, 25);
                    ImGui.Text(room.MapName);
                    ImGui.SameLine();
                    if (ImGui.Button("Remove"))
                        toRemove = room;
                    ImGui.DragInt2("Position", ref room.Position.X);

                    ImGui.Separator();

                    ImGui.PopID();
                }
                if (toRemove != null)
                    world.Rooms.Remove(toRemove);

                ImGui.Separator();
                if (ImGui.BeginCombo("Map", _room))
                {
                    foreach (var room in _rooms)
                    {
                        if (ImGui.Selectable(room, room == _room))
                            _room = room;
                    }
                    ImGui.EndCombo();
                }
                if (ImGui.Button("Add Room") && !string.IsNullOrEmpty(_room))
                {
                    scene.World.Rooms.Add(new Room
                    {
                        RoomName = Path.GetFileNameWithoutExtension(_room),
                        MapName = _room,
                    });
                }

                ImGui.End();
            }
        }
    }

    class World
    {
        [JsonExclude]
        public string Name = string.Empty;
        public List<Room> Rooms = new List<Room>();
        public List<Background> Backgrounds = new List<Background>();
        public string StartRoomId;
    }

    class Room
    {
        public string Id = Utils.RandomString();
        public string RoomName;
        public string MapName;
        public Point Position;
    }

    class Background
    {
        public string Id = Utils.RandomString();
        public string Filename;
        public Vector2 ScrollScale;
    }

    class WorldRenderer : RenderableComponent, IUpdatable
    {
        public override RectangleF Bounds
        {
            get
            {
                return new RectangleF(
                    new Vector2(float.MinValue / 2),
                    new Vector2(float.MaxValue));
            }
        }

        World _lastWorld;
        Dictionary<string, List<MapRenderer>> _renderers = new Dictionary<string, List<MapRenderer>>();
        new Dictionary<string, BoxCollider> _bounds = new Dictionary<string, BoxCollider>();

        public string GetRoomAt(Vector2 worldPosition)
        {
            foreach (var bounds in _bounds)
            {
                if (bounds.Value.Bounds.Contains(worldPosition))
                    return bounds.Key;
            }
            return null;
        }

        public void Update()
        {
            var scene = Entity.Scene as EditorScene;
            var world = scene.World;

            if (world != _lastWorld || world?.Rooms.Count != _renderers.Count)
            {
                foreach (var renderers in _renderers.Values)
                {
                    foreach (var renderer in renderers)
                        Entity.RemoveComponent(renderer);
                }
                _renderers.Clear();

                foreach (var bounds in _bounds.Values)
                    Entity.RemoveComponent(bounds);
                _bounds.Clear();

                if (world != null)
                {
                    foreach (var room in world.Rooms)
                    {
                        // TODO: put this in some content location
                        var ogmoProjectStr = File.ReadAllText($"{ContentPath.Maps}Metroidvania.ogmo");
                        var ogmoProject = Json.FromJson<OgmoProject>(ogmoProjectStr);

                        string ogmoLevelStr;
                        try
                        {
                            ogmoLevelStr = File.ReadAllText($"{ContentPath.Maps}{scene.WorldName}/{room.MapName}");
                        }
                        catch (FileNotFoundException)
                        {
                            Debug.Log($"{room.MapName} not found. Make sure file names are correct.");
                            continue;
                        }

                        var ogmoLevel = Json.FromJson<OgmoLevel>(ogmoLevelStr);

                        var renderers = new List<MapRenderer>();
                        for (var i = 0; i < ogmoLevel.layers.Count; ++i)
                        {
                            if (ogmoLevel.layers[i].data != null)
                            {
                                var renderer = Entity.AddComponent(new MapRenderer(ogmoProject, ogmoLevel, i));
                                renderer.SetRenderLayer(i);
                                renderers.Add(renderer);
                            }
                        }
                        _renderers.Add(room.Id, renderers);

                        var bounds = Entity.AddComponent(new BoxCollider(room.Position.X, room.Position.Y, ogmoLevel.width, ogmoLevel.height));
                        _bounds.Add(room.Id, bounds);
                    }
                }
            }

            if (world != null)
            {
                foreach (var room in world.Rooms)
                {
                    var renderers = _renderers[room.Id];
                    foreach (var renderer in renderers)
                        renderer.LocalOffset = room.Position.ToVector2();

                    var bounds = _bounds[room.Id];
                    bounds.SetLocalOffset(room.Position.ToVector2() + bounds.Bounds.Size / 2);
                }
            }

            _lastWorld = world;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            var roomIds = Entity.Scene.FindComponentOfType<EditorController>().RoomSelection;
            foreach (var id in roomIds)
            {
                var bounds = _bounds[id];
                batcher.DrawHollowRect(bounds.Bounds, Color.Green, 2);
            }
        }
    }

    class EditorController : Component, IUpdatable
    {
        public List<string> RoomSelection = new List<string>();

        SubpixelVector2 _subpixelV2;
        //Vector2 _selectionDelta;

        public override void OnAddedToEntity()
        {
            var scene = Entity.Scene as EditorScene;
            scene.OnWorldSet += HandleWorldSet;
        }

        public override void OnRemovedFromEntity()
        {
            var scene = Entity.Scene as EditorScene;
            scene.OnWorldSet -= HandleWorldSet;
        }

        void HandleWorldSet(EditorScene scene)
        {
            RoomSelection.Clear();
        }

        public void Update()
        {
            if (Input.MiddleMouseButtonDown)
            {
                var camera = Entity.Scene.Camera;
                var delta = Input.ScaledMousePositionDelta;
                delta /= camera.RawZoom;
                _subpixelV2.Update(ref delta);
                camera.Position -= delta;
            }

            if (Input.IsKeyPressed(Keys.Escape))
            {
                RoomSelection.Clear();
            }

            if (Input.LeftMouseButtonPressed)
            {
                if (!Input.IsKeyDown(Keys.LeftShift) && !Input.IsKeyDown(Keys.LeftShift))
                    RoomSelection.Clear();

                var worldRenderer = Entity.Scene.FindComponentOfType<WorldRenderer>();
                var roomId = worldRenderer.GetRoomAt(Entity.Scene.Camera.MouseToWorldPoint());
                if (roomId != null)
                    RoomSelection.Add(roomId);
            }

            var scene = Entity.Scene as EditorScene;
            var currWorld = scene.World;

            if (Input.RightMouseButtonDown && currWorld != null)
            {

                var delta = Input.ScaledMousePositionDelta;
                foreach (var roomId in RoomSelection)
                {
                    var room = currWorld.Rooms.Find(r => r.Id == roomId);
                    room.Position += delta.ToPoint();
                }
            }

            if (Input.RightMouseButtonReleased && currWorld != null)
            {
                foreach (var roomId in RoomSelection)
                {
                    var room = currWorld.Rooms.Find(r => r.Id == roomId);
                    room.Position.X = (int)Mathf.RoundToNearest(room.Position.X, 16);
                    room.Position.Y = (int)Mathf.RoundToNearest(room.Position.Y, 16);
                }
            }
        }
    }

    [EditorWindow]
    class BackgroundWindow : Component
    {
        List<string> _backgrounds;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            _backgrounds = new List<string>();
            var dirInfo = new DirectoryInfo(ContentPath.Backgrounds);
            foreach (var bgFile in dirInfo.GetFiles())
                _backgrounds.Add(bgFile.Name);
        }

        public override void OnRemovedFromEntity()
        {
            _backgrounds = null;
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            var scene = Entity.Scene as EditorScene;
            var currWorld = scene.World;
            if (currWorld == null) return;

            if (ImGui.Begin("Background"))
            {
                if (ImGui.Button("Add background"))
                {
                    currWorld.Backgrounds.Add(new Background());
                }

                Background toRemove = null;
                foreach (var background in currWorld.Backgrounds)
                {
                    ImGui.PushID(background.Id);
                    if (ImGui.BeginCombo("File", background.Filename))
                    {
                        foreach (var bg in _backgrounds)
                        {
                            if (ImGui.Selectable(bg, bg == background.Filename))
                                background.Filename = bg;
                        }
                        ImGui.EndCombo();
                    }

                    var scale = background.ScrollScale.ToNumerics();
                    if (ImGui.DragFloat2("Scroll Scale", ref scale))
                        background.ScrollScale = scale.ToXNA();

                    if (ImGui.Button("Remove"))
                        toRemove = background;

                    ImGui.Separator();
                    ImGui.PopID();
                }

                if (toRemove != null)
                    currWorld.Backgrounds.Remove(toRemove);

                ImGui.End();
            }
        }
    }
}
