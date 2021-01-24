using Nez;
using Nez.Persistence;
using System.IO;

namespace Game
{
    static class SaveSystem2
    {
        public class SaveFile
        {
            public string RoomId = "VFVTMVSAZGHVCLIFQHNSSYNCTKPZPVFIGXIXJV";
            public string CheckpointId = "EUTJBZWZVREDYQDPGUVCNQMHHDWPFCJFPTDUYQ";
        }

        public static void Save(int slotIndex, string roomId, string checkpointId)
        {
            var saveFile = new SaveFile
            {
                RoomId = roomId,
                CheckpointId = checkpointId,
            };
            var serialized = Json.ToJson(saveFile, new JsonSettings
            {
                PrettyPrint = true,
            });
            File.WriteAllText(GameFile.GetSavePath(slotIndex), serialized);
            Debug.Log("Game saved.");
        }

        public static void Save(int slotIndex, SaveFile saveFile = null)
        {
            saveFile = saveFile ?? new SaveFile();
            var serialized = Json.ToJson(saveFile, new JsonSettings
            {
                PrettyPrint = true,
            });
            File.WriteAllText(GameFile.GetSavePath(slotIndex), serialized);
            Debug.Log("Game saved.");
        }

        public static SaveFile Load(int slotIndex)
        {
            SaveFile data;
            try
            {
                var serialized = File.ReadAllText(GameFile.GetSavePath(slotIndex));
                data = Json.FromJson<SaveFile>(serialized);
            }
            catch (FileNotFoundException)
            {
                data = new SaveFile();
            }
            return data;
        }

        public static void Delete(int slotIndex)
        {
            File.Delete(GameFile.GetSavePath(slotIndex));
            Debug.Log($"Save slot #{slotIndex} deleted.");
        }
    }
}
