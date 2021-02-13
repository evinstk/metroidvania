using Nez.Console;

namespace Game
{
    public class Game : Nez.Core
    {
        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            ExitOnEscapeKeypress = false;
            DebugConsole.RenderScale = 2;

            Scene = new MainScene();
        }
    }
}
