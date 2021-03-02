using Microsoft.Xna.Framework;
using Nez;
using Nez.Console;
using System;

namespace Game
{
    public class Game : Core
    {
        public FMOD.Studio.System FMOD;

        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            ExitOnEscapeKeypress = false;
            DebugConsole.RenderScale = 2;
            //IsMouseVisible = false;

            FMOD = this.InitializeFMOD();
            this.LoadBank("Master");
            this.LoadBank("Master.strings");

            Scene = new MainScene();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            FMOD.update();
        }
    }

    static class CoreExt
    {
        public static FMOD.Studio.System InitializeFMOD(this Core _)
        {
            FMOD.Studio.System.create(out var fmodSystem);
            var systemResult = fmodSystem.initialize(8, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            Insist.IsTrue(systemResult == FMOD.RESULT.OK);

            return fmodSystem;
        }

        public static FMOD.Studio.System GetFMODSystem(this Core core)
        {
            if (core is Game game)
                return game.FMOD;
            if (core is EditorCore editor)
                return editor.FMOD;
            throw new Exception("No FMOD system");
        }
    }
}
