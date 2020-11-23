using Newtonsoft.Json;
using Nez;
using System;
using System.IO;

namespace Game
{
    class SaveSystem
    {
        static string _saveDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Metroidvania/";
        static string _saveFile = "save.json";
        static string _savePath = _saveDir + _saveFile;

        public SaveSystem()
        {
            CheckpointComponent.OnCheckpoint += Save;
        }

        void Save(CheckpointPayload checkpoint)
        {
            Directory.CreateDirectory(_saveDir);
            var save = JsonConvert.SerializeObject(checkpoint, Formatting.Indented);
            using (StreamWriter file = File.CreateText(_savePath))
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
                using (var sr = File.OpenText(_savePath))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    payload = new JsonSerializer().Deserialize<CheckpointPayload>(jsonTextReader);
                }
            }
            catch (FileNotFoundException)
            {
                payload = new CheckpointPayload
                {
                    MapSrc = "Start.tmx",
                    Name = "start",
                };
            }
            return payload;
        }
    }
}
