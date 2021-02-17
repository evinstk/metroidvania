using Nez;
using System.Collections.Generic;

namespace Game
{
    class Inventory
    {
        public List<WeaponTypes> Weapons = new List<WeaponTypes>();
        public int EquippedWeaponIndex = -1;
        public Weapon EquippedWeapon => EquippedWeaponIndex > -1
            ? Weapon.Types[Weapons[EquippedWeaponIndex]]
            : null;
    }

    static class InventoryExt
    {
        public static Inventory GetPlayerInventory(this Scene scene) => scene.GetScriptVars().Get<Inventory>(Vars.PlayerInventory);
    }
}
