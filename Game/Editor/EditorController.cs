using Nez;

namespace Game.Editor
{
    class EditorController : Component, IUpdatable
    {
        public void Update()
        {
            if (Input.LeftMouseButtonDown)
            {
                Entity.Scene.Camera.Position -= Input.ScaledMousePositionDelta;
            }
        }
    }
}
