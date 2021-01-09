using Game.Editor;
using Nez;
using Nez.Console;
using System;

namespace Game
{
    class RoomCore : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = true;
            DebugConsole.RenderScale = 2f;
            ExitOnEscapeKeypress = false;

            var managerTypes = ReflectionUtils.GetAllSubclasses(typeof(Manager), true);
            foreach (var managerType in managerTypes)
            {
                var manager = Activator.CreateInstance(managerType) as Manager;
                RegisterGlobalManager(manager);
            }

            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

            Scene = new RoomScene();
        }
    }
}
