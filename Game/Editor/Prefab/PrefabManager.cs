namespace Game.Editor.Prefab
{
    class PrefabManager : Manager<PrefabData>
    {
        public override string Path => ContentPath.Prefabs;
    }
}
