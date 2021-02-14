using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    class EditorScene : Scene
    {
        public string WorldName = string.Empty;
        public World World => Worlds.ContainsKey(WorldName) ? Worlds[WorldName] : null;
        public Dictionary<string, World> Worlds = new Dictionary<string, World>();

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
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class EditorWindowAttribute : Attribute
    {
    }

    [EditorWindow]
    class WorldWindow : Component
    {
        List<string> _worlds;

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
                ImGui.End();
            }
        }

        public void SetWorld(string worldName)
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
                Room toRemove = null;
                foreach (var room in world.Rooms)
                {
                    ImGui.PushID(room.Id);

                    ImGui.Text(room.MapName);
                    ImGui.SameLine();
                    if (ImGui.Button("Remove"))
                        toRemove = room;
                    ImGui.DragInt2("Position", ref room.Position.X);

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
                if (ImGui.Button("Add Room"))
                {
                    scene.World.Rooms.Add(new Room
                    {
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
    }

    class Room
    {
        public string Id = Utils.RandomString();
        public string MapName;
        public Point Position;
    }

    class WorldRenderer : Component, IUpdatable
    {
        World _lastWorld;
        Dictionary<string, List<MapRenderer>> _renderers = new Dictionary<string, List<MapRenderer>>();

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
                }
            }

            _lastWorld = world;
        }
    }

    class EditorController : Component, IUpdatable
    {
        SubpixelVector2 _subpixelV2;

        public void Update()
        {
            if (Input.LeftMouseButtonDown)
            {
                var camera = Entity.Scene.Camera;
                var delta = Input.ScaledMousePositionDelta;
                delta /= camera.RawZoom;
                _subpixelV2.Update(ref delta);
                camera.Position -= delta;
            }
        }
    }
}
