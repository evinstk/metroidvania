using Nez;
using Nez.Console;
using Nez.ImGuiTools;
using System.Collections.Generic;

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

            Screen.SetSize(Screen.MonitorWidth, Screen.MonitorHeight);

            Scene = new RoomEditorScene();
        }

        static List<Manager> _managers = new List<Manager>();
        public static T GetManager<T>() where T : Manager, new()
        {
            foreach (var manager in _managers)
            {
                if (manager is T typedManager)
                    return typedManager;
            }
            var newManager = new T();
            _managers.Add(newManager);
            return newManager;
        }
    }
}
