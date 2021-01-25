using Nez;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    class IntValue : ScriptableObject
    {
        public int InitialValue;
        [NotInspectable]
        [JsonExclude]
        public int RuntimeValue;

        public override void OnStart()
        {
            RuntimeValue = InitialValue;
        }
    }

    [CustomInspector(typeof(ReferenceInspector<IntValue>))]
    class IntReference : Reference<IntValue> {}
}
