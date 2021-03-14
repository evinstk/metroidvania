using Nez;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    class Save
    {
        public string World = "Intro";
        public string Room = "stasis_chambers";
        public string Checkpoint;

        public int MaxHealth = 5;

        public List<string> Weapons = new List<string>();
        public int EquippedWeaponIndex = -1;
        public List<string> RangedWeapons = new List<string>();
        public int EquippedRangedWeaponIndex = -1;

        public List<string> OpenChests = new List<string>();
    }

    class SaveSystem : GlobalManager
    {
        public event Action<SaveSystem> OnSave;

        public void Save(int saveSlot, string world, string room, string checkpoint)
        {
            var mainScene = (MainScene)Core.Scene;
            Insist.IsNotNull(mainScene);
            var vars = mainScene.GetScriptVars();

            var inventory = vars.Get<Inventory>(Vars.PlayerInventory);
            var weapons = new List<string>(inventory.Weapons.Count);
            foreach (var weapon in inventory.Weapons)
                weapons.Add(weapon.Name);
            var rangedWeapons = new List<string>(inventory.RangedWeapons.Count);
            foreach (var rangedWeapon in inventory.RangedWeapons)
                rangedWeapons.Add(rangedWeapon.Name);

            var save = new Save
            {
                World = world,
                Room = room,
                Checkpoint = checkpoint,

                MaxHealth = vars.Get<int>(Vars.PlayerMaxHealth),

                Weapons = weapons,
                EquippedWeaponIndex = inventory.EquippedWeaponIndex,
                RangedWeapons = rangedWeapons,
                EquippedRangedWeaponIndex = inventory.EquippedRangedWeaponIndex,

                OpenChests = vars.Get<List<string>>(Vars.OpenChests),
            };

            var serialized = Json.ToJson(save, new JsonSettings
            {
                PrettyPrint = true,
            });
            File.WriteAllText(GamePath.GetSavePath(saveSlot), serialized);
            Debug.Log("Game saved.");
            OnSave?.Invoke(this);
        }

        public Save Load(int saveSlot)
        {
            Save save;
            try
            {
                var serialized = File.ReadAllText(GamePath.GetSavePath(saveSlot));
                save = Json.FromJson<Save>(serialized);
            }
            catch (FileNotFoundException)
            {
                Debug.Log($"No save at slot {saveSlot}. Creating default save.");
                save = new Save();
            }
            return save;
        }
    }
}
