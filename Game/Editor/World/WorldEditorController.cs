using Nez;

namespace Game.Editor.World
{
    class WorldEditorController : Component, IUpdatable
    {
        SubpixelVector2 _subpixelV2;

        public void Update()
        {
            if (Input.LeftMouseButtonDown)
            {
                var camera = Entity.Scene.Camera;
                var delta = Input.ScaledMousePositionDelta;
                delta /= camera.RawZoom;
                _subpixelV2.Update(ref delta);
                camera.Position -= delta;
            }
        }
    }
}
