using Game.Editor.Scriptable;
using Nez;
using Nez.Persistence;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Game
{
    static class SaveSystem2
    {
        public class SaveFile
        {
            public string RoomId;
            public string CheckpointId;
            public List<ScriptableObject> ScriptableObjects = new List<ScriptableObject>();

            public SaveFile()
            {
                var soTypes = ReflectionUtils.GetAllSubclasses(typeof(ScriptableObject));
                foreach (var soType in soTypes)
                {
                    var manager = typeof(Core)
                        .GetMethod(nameof(Core.GetGlobalManager), BindingFlags.Public | BindingFlags.Static)
                        .MakeGenericMethod(typeof(ScriptableObjectManager<>).MakeGenericType(soType))
                        .Invoke(null, null);
                    manager.GetType().GetMethod("AddResourcesIncludedInSave").Invoke(manager, new[] { ScriptableObjects });
                }
            }
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
                TypeNameHandling = TypeNameHandling.All,
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
                TypeNameHandling = TypeNameHandling.All,
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

        public static bool Exists(int slotIndex)
        {
            return File.Exists(GameFile.GetSavePath(slotIndex));
        }
    }
}
