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
        public static FMOD.Studio.System InitializeFMOD(this Core core)
        {
            FMOD.Studio.System.create(out var fmodSystem);
            var systemResult = fmodSystem.initialize(8, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            Insist.IsTrue(systemResult == FMOD.RESULT.OK);

            //var debugResult = FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.FILE, null, "fmod_log.txt");
            //Insist.IsTrue(debugResult == FMOD.RESULT.OK);

            //var bankResult = fmodSystem.loadBankFile($"{ContentPath.FMOD}Master.bank", FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out var stuff);
            //Insist.IsTrue(bankResult == FMOD.RESULT.OK);
            //bankResult = fmodSystem.loadBankFile($"{ContentPath.FMOD}Master.strings.bank", FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out var stringBank);
            //Insist.IsTrue(bankResult == FMOD.RESULT.OK);

            //core.LoadBank("Master");
            //core.LoadBank("Master.strings");

            //bankResult = fmodSystem.loadBankFile($"{ContentPath.FMOD}Common.bank", FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out var bank);
            //Insist.IsTrue(bankResult == FMOD.RESULT.OK);

            //bank.loadSampleData();
            //bank.getEventList(out var events);
            //events[1].createInstance(out var instance);
            ////instance.setParameterByName("RPM", 500);
            //var startResult = instance.start();
            //Insist.IsTrue(startResult == FMOD.RESULT.OK);

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
