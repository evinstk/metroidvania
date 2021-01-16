using System;

namespace Game.Editor.Prefab
{
    class PrefabManager : Manager<PrefabData>
    {
        public override string Path => ContentPath.Prefabs;

        public event Action OnPrefabChange;
        public void TriggerPrefabChange() => OnPrefabChange?.Invoke();
    }
}
