using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class SpawnerData : DataComponent
    {
        public PrefabCombo Prefab = new PrefabCombo();
        public Vector2 Offset;
        public string EntityName = string.Empty;

        public override void AddToEntity(Entity entity)
        {
            var spawner = entity.AddComponent<Spawner>();
            spawner.Prefab = Core.GetGlobalManager<PrefabManager>().GetResource(Prefab.PrefabId);
            spawner.Offset = Offset;
            spawner.EntityName = EntityName;
        }
    }

    class Spawner : Component
    {
        public PrefabData Prefab;
        public Vector2 Offset;
        public string EntityName;

        public Entity Spawn(bool flip = false)
        {
            var entity = Prefab.CreateEntity(
                string.IsNullOrEmpty(EntityName) ? Prefab.Name : EntityName,
                Entity.Scene);
            entity.Position = Entity.Position + new Vector2(flip ? -Offset.X : Offset.X, Offset.Y);
            return entity;
        }
    }
}
