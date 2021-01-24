using Nez;

namespace Game.Editor.Scriptable
{
    class IntValue : ScriptableObject
    {
        public int Value;
    }

    [CustomInspector(typeof(ReferenceInspector<IntValue>))]
    class IntReference : Reference<IntValue> {}
}
