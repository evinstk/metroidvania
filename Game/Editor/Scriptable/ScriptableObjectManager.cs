using System.Collections.Generic;

namespace Game.Editor.Scriptable
{
    class ScriptableObjectManager<T> : Manager<T>
        where T : ScriptableObject, IResource, new()
    {
        public override string Path => ContentPath.ScriptableObjects + typeof(T).Name + "/";

        public void OnStart()
        {
            foreach (var resource in _resources)
                resource.Data.OnStart();
        }

        public void AddResourcesIncludedInSave(List<ScriptableObject> list)
        {
            foreach (var resource in _resources)
            {
                if (resource.Data.IncludeInSave)
                    list.Add(resource.Data);
            }
        }
    }
}
