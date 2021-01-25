using Nez;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    class FloatValue : ScriptableObject, IValue<float>
    {
        [JsonInclude]
        public float InitialValue { get; set; }
        [NotInspectable]
        [JsonInclude]
        public float RuntimeValue { get; set; }

        public override void OnStart()
        {
            RuntimeValue = InitialValue;
        }
    }

    [CustomInspector(typeof(ReferenceInspector<FloatValue>))]
    class FloatReference : Reference<FloatValue> {}
}
