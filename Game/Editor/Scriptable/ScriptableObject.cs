using Nez;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;

namespace Game.Editor.Scriptable
{
    abstract class ScriptableObject : IResource
    {
        [JsonInclude]
        [NotInspectable]
        public string Id { get; set; } = Utils.RandomString();
        public string DisplayName => Name;
        public string Name = "New Scriptable Object";
        public bool IncludeInSave;

        public virtual void OnStart() { }
    }

    abstract class Reference<T>
        where T : ScriptableObject, new()
    {
        public string Id;
        public T Dereference()
        {
            var resource = Core.GetGlobalManager<ScriptableObjectManager<T>>().GetResource(Id);
            Debug.LogIf(resource == null, $"{typeof(T).Name} not found. Supplying default value.");
            return resource ?? new T();
        }
    }

    class ReferenceInspector<T> : AbstractTypeInspector
        where T : ScriptableObject, new()
    {
        public override void DrawMutable()
        {
            var val = GetValue<Reference<T>>();
            var manager = Core.GetGlobalManager<ScriptableObjectManager<T>>();
            var id = val.Id;
            if (manager.Combo(_memberInfo.Name, ref id))
            {
                val.Id = id;
            }
        }
    }

    interface IValue<T>
    {
        T InitialValue { get; set; }
        T RuntimeValue { get; set; }
    }
}
