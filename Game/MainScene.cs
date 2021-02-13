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
            this.CreatePlayer(new Vector2(32, 32));

            var map = CreateEntity("map");

            var ogmoLevelStr = File.ReadAllText($"{ContentPath.Maps}level.json");
            var ogmoLevel = Json.FromJson<OgmoLevel>(ogmoLevelStr);
            foreach (var layer in ogmoLevel.layers)
            {
                map.AddComponent(new MapCollider(layer, ogmoLevel.width, ogmoLevel.height));
            }

            //var ogmoProjectStr = File.ReadAllText(ContentPath.Maps + "Metroidvania.ogmo");
            //var ogmoProject = Json.FromJson<OgmoProject>(ogmoProjectStr);
        }
    }
}
