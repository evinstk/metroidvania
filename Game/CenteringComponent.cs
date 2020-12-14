using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class CenteringComponent : Component, IUpdatable
    {
        public void Update()
        {
            var pos = Entity.Scene.Camera.ScreenToWorldPoint(Screen.Center) * new Vector2(
                (float)MainScene.ResWidth / MainScene.ScreenWidth,
                (float)MainScene.ResHeight / MainScene.ScreenHeight);
            Entity.Position = pos;
        }
    }
}
