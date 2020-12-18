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
            if (Input.MouseWheelDelta !=0)
            {
                var value = Input.MouseWheelDelta * .005f;
                Entity.Scene.Camera.RawZoom += value;
            }
        }
    }
}
