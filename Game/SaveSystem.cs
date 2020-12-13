using Newtonsoft.Json;
using Nez;
using System.IO;

namespace Game
{
    class SaveSystem
    {
        public SaveSystem()
        {
            CheckpointComponent.OnCheckpoint += Save;
        }

        void Save(CheckpointPayload checkpoint)
        {
            var save = JsonConvert.SerializeObject(checkpoint, Formatting.Indented);
            using (StreamWriter file = File.CreateText(GameFile.SavePath))
            {
                file.WriteLine(save);
            }
            Debug.Log("File saved.");
        }

        public CheckpointPayload Load()
        {
            CheckpointPayload payload;
            try
            {
                using (var sr = File.OpenText(GameFile.SavePath))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    payload = new JsonSerializer().Deserialize<CheckpointPayload>(jsonTextReader);
                }
            }
            catch (FileNotFoundException)
            {
                payload = new CheckpointPayload
                {
                    MapSrc = "Map8.tmx",
                    Name = "start",
                };
            }
            return payload;
        }
    }
}
