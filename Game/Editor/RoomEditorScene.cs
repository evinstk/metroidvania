using Game.Editor.Animation;
using Game.Editor.Tool;
using Microsoft.Xna.Framework;
using Nez;
using System;

namespace Game.Editor
{
    class RoomEditorScene : Scene
    {
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
            var windows = CreateEntity("windows");
            foreach (var windowType in ReflectionUtils.GetAllTypesWithAttribute<EditorWindowAttribute>())
            {
                var window = Activator.CreateInstance(windowType) as Component;
                Insist.IsNotNull(window);
                windows.AddComponent(window);
            }
        }
    }
}
