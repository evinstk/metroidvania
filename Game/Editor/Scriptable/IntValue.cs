using Nez;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    class IntValue : ScriptableObject, IValue<int>
    {
        [JsonInclude]
        public int InitialValue { get; set; }
        [NotInspectable]
        [JsonInclude]
        public int RuntimeValue { get; set; }

        public override void OnStart()
        {
            RuntimeValue = InitialValue;
        }
    }

    [CustomInspector(typeof(ReferenceInspector<IntValue>))]
    class IntReference : Reference<IntValue> {}
}
