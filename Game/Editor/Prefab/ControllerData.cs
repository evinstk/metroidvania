using Nez;

namespace Game.Editor.Prefab
{
    class PlayerControllerData : PrefabComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<PlayerController>();
        }
    }
}
