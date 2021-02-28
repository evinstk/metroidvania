using Microsoft.Xna.Framework;
using Nez;
using Nez.Console;
using Nez.ImGuiTools;

namespace Game
{
    class EditorCore : Core
    {
        public FMOD.Studio.System FMOD;

        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            ExitOnEscapeKeypress = false;
            DebugConsole.RenderScale = 2;

            var imGuiManager = new ImGuiManager();
            RegisterGlobalManager(imGuiManager);

            Screen.SetSize(Screen.MonitorWidth, Screen.MonitorHeight);

            FMOD = this.InitializeFMOD();
            this.LoadBank("Master");
            this.LoadBank("Master.strings");

            Scene = new EditorScene();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            FMOD.update();
        }
    }
}
