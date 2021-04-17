﻿using Game.Entities;
using Game.Scripts;
using Game.Worlds;
using Microsoft.Xna.Framework;
using Nez;

namespace Game.Prototypes
{
    class PrototypeScene : Scene
    {
        string _scriptPath;

        public override void Initialize()
        {
            ClearColor = Color.Black;
        }

        public PrototypeScene(string scriptPath)
        {
            _scriptPath = scriptPath;
        }

        public override void OnStart()
        {
            AddSceneComponent<EntityLoader>();
            AddSceneComponent<WorldLoader>();

            var scriptLoader = AddSceneComponent<ScriptLoader>();
            scriptLoader.LoadScript(_scriptPath);
        }
    }
}
