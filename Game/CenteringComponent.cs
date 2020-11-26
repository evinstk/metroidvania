using Nez;

namespace Game
{
    class CenteringComponent : Component, IUpdatable
    {
        public void Update()
        {
            Entity.Position = Entity.Scene.Camera.ScreenToWorldPoint(Screen.Center);
        }
    }
}
