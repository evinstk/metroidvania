using Nez;

namespace Game.Editor
{
    class EditorController : Component, IUpdatable
    {
        SubpixelVector2 _subpixelV2;

        public void Update()
        {
            if (Input.LeftMouseButtonDown)
            {
                var delta = Input.ScaledMousePositionDelta;
                _subpixelV2.Update(ref delta);
                Entity.Scene.Camera.Position -= delta;
            }
            if (Input.MouseWheelDelta !=0)
            {
                var value = Input.MouseWheelDelta * .005f;
                Entity.Scene.Camera.RawZoom += value;
            }
        }
    }
}
