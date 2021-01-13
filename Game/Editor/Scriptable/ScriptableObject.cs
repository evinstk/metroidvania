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
    }

    abstract class Reference
    {
        public string Id;
    }

    class ReferenceInspector<T> : AbstractTypeInspector
        where T : ScriptableObject, new()
    {
        public override void DrawMutable()
        {
            var val = GetValue<Reference>();
            var manager = Core.GetGlobalManager<ScriptableObjectManager<T>>();
            var id = val.Id;
            if (manager.Combo(_memberInfo.Name, ref id))
            {
                val.Id = id;
            }
        }
    }
}
