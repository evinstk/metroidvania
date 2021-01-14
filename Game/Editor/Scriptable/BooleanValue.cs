using Nez;

namespace Game.Editor.Scriptable
{
    class BooleanValue : ScriptableObject
    {
        public bool Value;
    }

    [CustomInspector(typeof(ReferenceInspector<BooleanValue>))]
    class BooleanReference : Reference<BooleanValue> {}
}
