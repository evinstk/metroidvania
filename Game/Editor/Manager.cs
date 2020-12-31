using Game.Editor.Prefab;
using ImGuiNET;
using Nez;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Editor
{
    interface IResource
    {
        string Id { get; }
        string DisplayName { get; }
    }

    abstract class Manager
    {
        protected static JsonTypeConverter[] _typeConverters;
        static Manager()
        {
            var subclasses = ReflectionUtils.GetAllSubclasses(typeof(JsonTypeConverter), true);
            var converters = new List<JsonTypeConverter>();
            foreach (var subclass in subclasses)
                converters.Add(Activator.CreateInstance(subclass) as JsonTypeConverter);
            _typeConverters = converters.ToArray();
        }
    }

    abstract class Manager<T> : Manager
        where T : class, IResource
    {
        protected class ResourceMeta
        {
            public T Data;
            public string Filename;
        }

        public abstract string Path { get; }
        protected virtual string _searchPattern => "*.json";

        List<ResourceMeta> _resources = new List<ResourceMeta>();
        public Manager()
        {
            foreach (var f in Directory.GetFiles(Path, _searchPattern))
            {
                var resource = LoadResource(f);
                _resources.Add(new ResourceMeta
                {
                    Data = resource,
                    Filename = f,
                });
            }
        }

        protected virtual T LoadResource(string file)
        {
            var serialized = File.ReadAllText(file);
            var data = Json.FromJson<T>(serialized, new JsonSettings
            {
                TypeConverters = _typeConverters,
            });
            return data;
        }

        protected virtual void SaveResource(ResourceMeta meta)
        {
            var serialized = Json.ToJson(meta.Data, new JsonSettings
            {
                PrettyPrint = true,
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = true,
                TypeConverters = _typeConverters,
            });
            File.WriteAllText(meta.Filename, serialized);
        }

        public T GetResource(string id) => _resources.Find(r => r.Data.Id == id)?.Data;

        public void SaveAll()
        {
            foreach (var r in _resources)
            {
                SaveResource(r);
            }
        }

        #region ImGui

        public bool RadioButtons(string activeId, ref string selectedId)
        {
            var ret = false;
            foreach (var resource in _resources)
            {
                ImGui.PushID(resource.Data.Id);
                if (ImGui.RadioButton(resource.Data.DisplayName, resource.Data.Id == activeId))
                {
                    selectedId = resource.Data.Id;
                    ret = true;
                }
                ImGui.PopID();
            }
            return ret;
        }

        #endregion
    }
}
