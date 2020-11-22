using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nez;
using Nez.Console;
using System.IO;

namespace Game
{
    public class Game : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            InitOptions options;
            try
            {
                using (var stream = TitleContainer.OpenStream("config.json"))
                using (var sr = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    options = new JsonSerializer().Deserialize<InitOptions>(jsonTextReader);
                }
            }
            catch (FileNotFoundException)
            {
                // use default options
                options = new InitOptions();
            }
            // overwrite file even if exists in case new properties
            using (StreamWriter file = File.CreateText("config.json"))
            {
                var optionsSerialized = JsonConvert.SerializeObject(options, Formatting.Indented);
                file.WriteLine(optionsSerialized);
            }

            Window.AllowUserResizing = true;
            Screen.IsFullscreen = options.Fullscreen;
            ExitOnEscapeKeypress = true;
            DebugConsole.RenderScale = options.ConsoleRenderScale;
            Scene = new MainScene("Prison.tmx", "start");
        }

        public static Scene Transition(string transitionSrc, string spawn)
        {
            var health = Scene.FindEntity("player").GetComponent<HealthComponent>().Health;
            var nextScene = new MainScene(transitionSrc, spawn, health);
            return nextScene;
        }
    }

    class InitOptions
    {
        public bool Fullscreen = true;
        public float ConsoleRenderScale = 2f;
    }
}
