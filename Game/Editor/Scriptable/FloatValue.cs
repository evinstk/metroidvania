using Nez;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    class FloatValue : ScriptableObject
    {
        public float InitialValue;
        [NotInspectable]
        [JsonExclude]
        public float RuntimeValue;

        public override void OnStart()
        {
            RuntimeValue = InitialValue;
        }
    }

    [CustomInspector(typeof(ReferenceInspector<FloatValue>))]
    class FloatReference : Reference<FloatValue> {}
}
