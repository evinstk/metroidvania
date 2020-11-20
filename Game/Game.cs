using Nez;

namespace Game
{
    public class Game : Core
    {
        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            ExitOnEscapeKeypress = false;
            Scene = new MainScene("TestMap.tmx", "start");
        }

        public static Scene Transition(string transitionSrc, string spawn)
        {
            var health = Scene.FindEntity("player").GetComponent<HealthComponent>().Health;
            var nextScene = new MainScene(transitionSrc, spawn, health);
            return nextScene;
        }
    }
}
