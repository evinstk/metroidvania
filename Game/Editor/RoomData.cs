using Nez;
using Nez.Persistence;
using System;
using System.IO;

namespace Game.Editor
{
    [Serializable]
    class RoomData
    {
        public string Name = "";
        public int Width = 1;
        public int Height = 1;

        public void SaveToFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            var roomData = Json.ToJson(this, new JsonSettings
            {
                PrettyPrint = true,
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = true,
            });

            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine(roomData);
            }
        }

        public static RoomData ReadFromFile(string filename)
        {
            var roomDataStr = File.ReadAllText(filename);
            var roomData = Json.FromJson<RoomData>(roomDataStr);
            return roomData;
        }
    }

    //class RoomDataComponent : Component
    //{
    //    public RoomData RoomData = new RoomData
    //    {
    //        Name = "",
    //    };
    //}
}
