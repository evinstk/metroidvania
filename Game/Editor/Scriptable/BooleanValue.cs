using Nez;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    class BooleanValue : ScriptableObject, IValue<bool>
    {
        [JsonInclude]
        public bool InitialValue { get; set; }
        [NotInspectable]
        [JsonInclude]
        public bool RuntimeValue { get; set; }

        public override void OnStart()
        {
            RuntimeValue = InitialValue;
        }
    }

    [CustomInspector(typeof(ReferenceInspector<BooleanValue>))]
    class BooleanReference : Reference<BooleanValue> {}
}
