using Game.Editor.Tool;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor
{
    class RoomEditorScene : Scene
    {
        RoomMetadata _roomMetadata;

        public RoomEditorScene() { }

        public RoomEditorScene(RoomMetadata roomMetadata)
        {
            _roomMetadata = roomMetadata;
        }

        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            //Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

            //Time.TimeScale = 0;
            ClearColor = new Color(0xff371f0f);
        }

        public override void OnStart()
        {
            CreateWindows();
            CreateEntity("map-renderer").AddComponent<MapEditorRenderer>();
            CreateEntity("prefab-renderer").AddComponent<PrefabEditorRenderer>();
        }

        void CreateWindows()
        {
            var toolWindow = new ToolWindow();
            toolWindow.SetRenderLayer(-1);
            CreateEntity("windows")
                .AddComponent(new RoomWindow(_roomMetadata))
                .AddComponent(toolWindow)
                .AddComponent<TilesetWindow>()
                .AddComponent<Prefab.PrefabWindow>()
                .AddComponent<LayerWindow>()
                .AddComponent<TransportWindow>();
        }
    }
}
