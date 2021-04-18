using Game.Cinema;
using Game.Entities;
using Game.Scripts;
using Game.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game.Prototypes
{
    class PrototypeScene : Scene
    {
        string _scriptPath;

        public override void Initialize()
        {
            ClearColor = Color.DarkSlateGray;
        }

        public PrototypeScene(string scriptPath)
        {
            _scriptPath = scriptPath;
        }

        public override void OnStart()
        {
            AddSceneComponent<SceneSettings>();
            AddSceneComponent<EntityLoader>();
            AddSceneComponent<WorldLoader>();

            var scriptLoader = AddSceneComponent<ScriptLoader>();

            Camera.AddComponent<CameraBrain>();
            Camera.Entity.UpdateOrder = int.MaxValue;

            scriptLoader.LoadScript(_scriptPath);
        }

        public override void Update()
        {
            if (Input.IsKeyDown(Keys.F2))
            {
                GameContent.ClearCache();
                Core.Scene = new PrototypeScene(_scriptPath);
            }

            base.Update();
        }
    }
}
