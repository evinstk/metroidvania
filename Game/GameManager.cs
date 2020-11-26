using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    class GameManager : Component, IUpdatable
    {
        public void Update()
        {
            if (Input.IsKeyDown(Keys.Delete) || Input.GamePads[0].IsButtonDown(Buttons.Start))
            {
                var scene = Core.Scene as MainScene;
                scene.Reset();
            }
        }
    }
}
