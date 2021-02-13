using Microsoft.Xna.Framework;
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
        }

        public override void OnStart()
        {
            this.CreatePlayer(new Vector2(64, 64));
            this.CreateSentry(new Vector2(164, 200));

            var map = CreateEntity("map");

            var ogmoProjectStr = File.ReadAllText($"{ContentPath.Maps}Metroidvania.ogmo");
            var ogmoProject = Json.FromJson<OgmoProject>(ogmoProjectStr);

            var ogmoLevelStr = File.ReadAllText($"{ContentPath.Maps}level.json");
            var ogmoLevel = Json.FromJson<OgmoLevel>(ogmoLevelStr);
            for (var i = 0; i < ogmoLevel.layers.Count; ++i)
            {
                var layer = ogmoLevel.layers[i];
                if (layer.data != null)
                {
                    var renderer = map.AddComponent(new MapRenderer(ogmoProject, ogmoLevel, i));
                    renderer.RenderLayer = i;
                }
                if (layer.name == "terrain")
                    map.AddComponent(new MapCollider(layer, ogmoLevel.width, ogmoLevel.height));
            }
        }

        public override void Update()
        {
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
