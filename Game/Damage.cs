using Nez;

namespace Game
{
    class Damage : Component
    {
        public int Amount = 1;
    }

    static class DamageExt
    {
        public static int GetDamageAmount(this Entity entity)
        {
            var damage = entity.GetComponent<Damage>()?.Amount ?? 1;
            return damage;
        }

        public static int GetDamageAmount(this Component component)
        {
            return GetDamageAmount(component.Entity);
        }
    }
}
