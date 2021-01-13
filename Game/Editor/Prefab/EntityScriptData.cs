using Game.Scripting;
using Nez;

namespace Game.Editor.Prefab
{
    class EntityScriptData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<EntityScript>();
        }
    }
}
