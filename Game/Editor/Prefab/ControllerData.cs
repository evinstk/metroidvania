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

    class FreeControllerData : PrefabComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<FreeController>();
        }
    }
}
