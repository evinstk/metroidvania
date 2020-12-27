using Game.Editor;
using ImGuiNET;
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
            _roomMetadata = roomMetadata;
        }

        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
        }

        public override void OnStart()
        {
            if (_roomMetadata != null)
            {
                CreateEntity("windows")
                    .AddComponent(new RoomTransport(_roomMetadata));
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
