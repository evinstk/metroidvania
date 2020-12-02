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

            var opts = LoadConfig();

            Window.AllowUserResizing = true;
            Screen.IsFullscreen = opts.Fullscreen;
            ExitOnEscapeKeypress = true;
            DebugConsole.RenderScale = opts.ConsoleRenderScale;

            var saveSystem = new SaveSystem();
            var checkpoint = saveSystem.Load();

            var sceneOpts = new SceneOptions
            {
                UseLighting = opts.UseLighting,
            };
            Scene = new MainScene(checkpoint.MapSrc, checkpoint.Name, sceneOpts);
        }

        InitOptions LoadConfig()
        {
            InitOptions options;
            try
            {
                using (var sr = File.OpenText(GameFile.ConfigPath))
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
            var optionsSerialized = JsonConvert.SerializeObject(options, Formatting.Indented);
            using (StreamWriter file = File.CreateText(GameFile.ConfigPath))
            {
                file.WriteLine(optionsSerialized);
            }

            return options;
        }
    }

    class InitOptions
    {
        public bool Fullscreen = true;
        public float ConsoleRenderScale = 2f;
        public bool UseLighting = true;
    }
}
