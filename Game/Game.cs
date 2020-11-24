﻿using Newtonsoft.Json;
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

            LoadConfig();

            var saveSystem = new SaveSystem();
            var checkpoint = saveSystem.Load();

            Scene = new MainScene(checkpoint.MapSrc, checkpoint.Name);
        }

        void LoadConfig()
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

            Window.AllowUserResizing = true;
            Screen.IsFullscreen = options.Fullscreen;
            ExitOnEscapeKeypress = true;
            DebugConsole.RenderScale = options.ConsoleRenderScale;
        }

        static bool _transitioning = false;
        public static SceneTransition Transition(string transitionSrc, string spawn, int health)
        {
            if (_transitioning) return null;
            _transitioning = true;
            var nextScene = new MainScene(transitionSrc, spawn, health);
            var transition = StartSceneTransition(new FadeTransition(() => nextScene));
            transition.FadeOutDuration = 0.3f;
            transition.FadeInDuration = 0.2f;
            transition.OnTransitionCompleted += () => _transitioning = false;
            return transition;
        }
    }

    class InitOptions
    {
        public bool Fullscreen = true;
        public float ConsoleRenderScale = 2f;
    }
}
