using Nez;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    class BooleanValue : ScriptableObject
    {
        public bool InitialValue;
        [NotInspectable]
        [JsonExclude]
        public bool RuntimeValue;

        public override void OnStart()
        {
            RuntimeValue = InitialValue;
        }
    }

    [CustomInspector(typeof(ReferenceInspector<BooleanValue>))]
    class BooleanReference : Reference<BooleanValue> {}
}
