﻿using Nez;
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
            DebugConsole.RenderScale = 2f;
            ExitOnEscapeKeypress = false;

            var imGuiManager = new ImGuiManager();
            RegisterGlobalManager(imGuiManager);

            Scene = new EditorScene();
        }
    }
}