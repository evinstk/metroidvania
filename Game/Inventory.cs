using Nez;
using System;
using System.Collections.Generic;

namespace Game
{
    class Inventory
    {
        public event Action<Item> OnItemAdd;

        public List<Weapon> Weapons = new List<Weapon>();
        public int EquippedWeaponIndex = -1;
        public Weapon EquippedWeapon => EquippedWeaponIndex > -1
            ? Weapons[EquippedWeaponIndex]
            : null;

        public List<RangedWeapon> RangedWeapons = new List<RangedWeapon>();
        public int EquippedRangedWeaponIndex = -1;
        public RangedWeapon EquippedRangedWeapon => EquippedRangedWeaponIndex > -1
            ? RangedWeapons[EquippedRangedWeaponIndex]
            : null;

        public void Add(Item item)
        {
            if (item is Weapon weapon)
                Weapons.Add(weapon);
            if (item is RangedWeapon ranged)
                RangedWeapons.Add(ranged);
            OnItemAdd?.Invoke(item);
        }
    }

    static class InventoryExt
    {
        public static Inventory GetPlayerInventory(this Scene scene) => scene.GetScriptVars().Get<Inventory>(Vars.PlayerInventory);
    }
}
