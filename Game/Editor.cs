using Nez;
using Nez.Console;
using Nez.ImGuiTools;

namespace Game
{
    class Editor : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = true;
            DebugConsole.RenderScale = 2f;

            var imGuiManager = new ImGuiManager();
            RegisterGlobalManager(imGuiManager);

            Scene = new EditorScene();
        }
    }
}
