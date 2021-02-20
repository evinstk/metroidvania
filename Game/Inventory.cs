using Nez;
using System;
using System.Collections.Generic;

namespace Game
{
    class Inventory
    {
        public event Action<WeaponTypes> OnWeaponAdd;

        public List<WeaponTypes> Weapons = new List<WeaponTypes>();
        public int EquippedWeaponIndex = -1;
        public Weapon EquippedWeapon => EquippedWeaponIndex > -1
            ? Weapon.Types[Weapons[EquippedWeaponIndex]]
            : null;

        public void Add(WeaponTypes type)
        {
            Weapons.Add(type);
            OnWeaponAdd?.Invoke(type);
        }
    }

    static class InventoryExt
    {
        public static Inventory GetPlayerInventory(this Scene scene) => scene.GetScriptVars().Get<Inventory>(Vars.PlayerInventory);
    }
}
