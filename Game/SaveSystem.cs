using Nez;
using Nez.Persistence;
using System;
using System.IO;

namespace Game
{
    class Save
    {
        public string World;
        public string Room;
        public string Checkpoint;
    }

    class SaveSystem : GlobalManager
    {
        public event Action<SaveSystem> OnSave;

        public void Save(int saveSlot, string world, string room, string checkpoint)
        {
            var save = new Save
            {
                World = world,
                Room = room,
                Checkpoint = checkpoint,
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
            Save save = null;
            try
            {
                var serialized = File.ReadAllText(GamePath.GetSavePath(saveSlot));
                save = Json.FromJson<Save>(serialized);
            }
            catch (FileNotFoundException)
            {
                Debug.Log($"No save at slot {saveSlot}.");
            }
            return save;
        }
    }
}
