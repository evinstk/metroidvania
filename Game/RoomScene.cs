using Game.Editor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;

namespace Game
{
    class RoomScene : Scene
    {
        RoomMetadata _roomMetadata;

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
                layerEntity.AddComponent(new MapRenderer(roomData, i)).SetRenderLayer((count - 1 - i) * 10);
            }
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
