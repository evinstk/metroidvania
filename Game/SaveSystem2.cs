using Nez;
using Nez.Persistence;
using System.IO;

namespace Game
{
    static class SaveSystem2
    {
        public class SaveFile
        {
            public string RoomId = "ICLOHYCSXGMCKXQRKOULNXNVVHJKNWYYFGSXGO";
            public string CheckpointId = "LKSBKDXEHTMWQXJYMQATLDOVLNDCKPTTFGJZLI";
        }

        public static void Save(string roomId, string checkpointId)
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
            File.WriteAllText(GameFile.SavePath, serialized);
            Debug.Log("Game saved.");
        }

        public static SaveFile Load()
        {
            SaveFile data;
            try
            {
                var serialized = File.ReadAllText(GameFile.SavePath);
                data = Json.FromJson<SaveFile>(serialized);
            }
            catch (FileNotFoundException)
            {
                data = new SaveFile();
            }
            return data;
        }
    }
}
