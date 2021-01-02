using Nez;
using Nez.Console;
using Nez.ImGuiTools;
using System;

namespace Game.Editor
{
    class EditorCore : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = true;
            DebugConsole.RenderScale = 2f;
            ExitOnEscapeKeypress = false;

            var imGuiManager = new ImGuiManager();
            RegisterGlobalManager(imGuiManager);

            var managerTypes = ReflectionUtils.GetAllSubclasses(typeof(Manager), true);
            foreach (var managerType in managerTypes)
            {
                var manager = Activator.CreateInstance(managerType) as Manager;
                RegisterGlobalManager(manager);
            }

            Screen.SetSize(Screen.MonitorWidth, Screen.MonitorHeight);

            Scene = new RoomEditorScene();
        }
    }
}
