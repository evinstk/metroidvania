using Nez;

namespace Game.Editor.Scriptable
{
    class FloatValue : ScriptableObject
    {
        public float Value;
    }

    [CustomInspector(typeof(ReferenceInspector<FloatValue>))]
    class FloatReference : Reference {}
}
