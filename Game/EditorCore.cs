using Nez;
using Nez.Console;
using Nez.ImGuiTools;

namespace Game
{
    class EditorCore : Core
    {
        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            ExitOnEscapeKeypress = false;
            DebugConsole.RenderScale = 2;

            var imGuiManager = new ImGuiManager();
            RegisterGlobalManager(imGuiManager);

            Screen.SetSize(Screen.MonitorWidth, Screen.MonitorHeight);

            Scene = new EditorScene();
        }
    }
}
