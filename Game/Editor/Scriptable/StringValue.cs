using Nez;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    class StringValue : ScriptableObject, IValue<string>
    {
        [JsonInclude]
        public string InitialValue { get; set; } = string.Empty;
        [NotInspectable]
        [JsonInclude]
        public string RuntimeValue { get; set; } = string.Empty;

        public override void OnStart()
        {
            RuntimeValue = InitialValue;
        }
    }

    [CustomInspector(typeof(ReferenceInspector<StringValue>))]
    class StringReference : Reference<StringValue> {}
}
