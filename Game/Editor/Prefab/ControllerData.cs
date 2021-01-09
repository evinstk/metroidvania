using Nez;

namespace Game.Editor.Prefab
{
    class PlayerControllerData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<PlayerController>();
        }
    }

    class FreeControllerData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<FreeController>();
        }
    }
}
