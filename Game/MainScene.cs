using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Persistence;
using System.IO;

namespace Game
{
    class MainScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(Constants.ResWidth, Constants.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(Constants.ScreenWidth, Constants.ScreenHeight);
            ClearColor = Constants.ClearColor;

            Physics.SpatialHashCellSize = 16;
        }

        public override void OnStart()
        {
            Camera.AddComponent<CameraBounds>();
            Camera.Entity.UpdateOrder = int.MaxValue - 1;

            var map = CreateEntity("map");

            var ogmoProjectStr = File.ReadAllText($"{ContentPath.Maps}Metroidvania.ogmo");
            var ogmoProject = Json.FromJson<OgmoProject>(ogmoProjectStr);

            var ogmoLevelStr = File.ReadAllText($"{ContentPath.Maps}World1/0x0.json");
            var ogmoLevel = Json.FromJson<OgmoLevel>(ogmoLevelStr);
            for (var i = 0; i < ogmoLevel.layers.Count; ++i)
            {
                var layer = ogmoLevel.layers[i];
                if (layer.data != null)
                {
                    var renderer = map.AddComponent(new MapRenderer(ogmoProject, ogmoLevel, i));
                    renderer.RenderLayer = i;
                }
                else if (layer.entities != null)
                {
                    foreach (var entity in layer.entities)
                    {
                        switch (entity.name)
                        {
                            case "player":
                                if (FindComponentOfType<Player>() == null)
                                    this.CreatePlayer(new Vector2(entity.x, entity.y));
                                break;
                            case "sentry":
                                this.CreateSentry(new Vector2(entity.x, entity.y));
                                break;
                            default:
                                Debug.Log($"Unknown entity type {entity.name}");
                                break;
                        }
                    }
                }
                if (layer.name == "terrain")
                    map.AddComponent(new MapCollider(layer, ogmoLevel.width, ogmoLevel.height));
            }
        }

        public override void Update()
        {
            // restart
            if (Input.IsKeyDown(Keys.F2))
                Core.Scene = new MainScene();

            if (Timer.PauseTimer > 0)
            {
                Timer.PauseTimer -= Time.DeltaTime;
                if (Timer.PauseTimer <= -0.0001f)
                    Time.DeltaTime = -Timer.PauseTimer;
                else
                    return;
            }
            base.Update();
        }
    }
}
