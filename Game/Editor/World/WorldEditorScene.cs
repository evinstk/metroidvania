using Microsoft.Xna.Framework;
using Nez;
using System;

namespace Game.Editor.World
{
    class WorldEditorScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth * 4, MainScene.ResHeight * 4, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = new Color(0xff371f0f);

            var windows = CreateEntity("windows");
            foreach (var windowType in ReflectionUtils.GetAllTypesWithAttribute<WorldEditorWindowAttribute>())
            {
                var window = Activator.CreateInstance(windowType) as Component;
                Insist.IsNotNull(window);
                windows.AddComponent(window);
            }

            CreateEntity("world-room-renderer").AddComponent<WorldEditorRenderer>();
            CreateEntity("world-room-controller").AddComponent<WorldEditorController>();
        }
    }
}
