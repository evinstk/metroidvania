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

    abstract class Manager : GlobalManager
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
        where T : class, IResource, new()
    {
        protected class ResourceMeta
        {
            public T Data;
            public string Filename;
        }

        public abstract string Path { get; }
        protected virtual string _searchPattern => "*.json";

        protected List<ResourceMeta> _resources = new List<ResourceMeta>();
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

        public T NewResource(string filename = null)
        {
            var resourceMeta = new ResourceMeta
            {
                Data = new T(),
            };
            resourceMeta.Filename = Path + (filename ?? (resourceMeta.Data.Id + ".json"));
            SaveResource(resourceMeta);
            _resources.Add(resourceMeta);
            return resourceMeta.Data;
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

        public T GetResource(string id) => id != null ? _resources.Find(r => r.Data.Id == id)?.Data : null;

        public void SaveAll()
        {
            foreach (var r in _resources)
            {
                SaveResource(r);
            }
        }

        public void Save(T resource)
        {
            foreach (var r in _resources)
            {
                if (r.Data == resource)
                    SaveResource(r);
            }
        }

        public void Delete(T resource)
        {
            ResourceMeta meta = null;
            foreach (var r in _resources)
            {
                if (r.Data == resource)
                    meta = r;
            }
            Insist.IsNotNull(meta);
            if (meta != null)
            {
                File.Delete(meta.Filename);
                _resources.Remove(meta);
            }
        }

        public void Delete(string resourceId)
        {
            ResourceMeta meta = null;
            foreach (var r in _resources)
            {
                if (r.Data.Id == resourceId)
                    meta = r;
            }
            Insist.IsNotNull(meta);
            if (meta != null)
            {
                File.Delete(meta.Filename);
                _resources.Remove(meta);
            }
        }

        #region ImGui

        string _newResourceInput = "";
        public bool NewResourcePopup(out T newResource)
        {
            var ret = false;
            newResource = null;
            if (ImGui.BeginPopupModal("new-resource"))
            {
                ImGui.Text("File name:");
                ImGui.InputText("##newResourceName", ref _newResourceInput, 25);
                if (ImGui.Button("Cancel"))
                {
                    _newResourceInput = "";
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("Save") && _newResourceInput.Length > 0)
                {
                    newResource = NewResource(_newResourceInput);
                    _newResourceInput = "";
                    ret = true;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            return ret;
        }

        public bool NewResourcePopup()
        {
            return NewResourcePopup(out _);
        }

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

        public bool RadioButtons(ref string selectedId) => RadioButtons(selectedId, ref selectedId);

        public bool Combo(string label, string activeId, ref string selectedId)
        {
            var ret = false;
            var active = GetResource(activeId);
            if (ImGui.BeginCombo(label, active?.DisplayName))
            {
                foreach (var resource in _resources)
                {
                    if (ImGui.Selectable(resource.Data.DisplayName))
                    {
                        selectedId = resource.Data.Id;
                        ret = true;
                    }
                }
                ImGui.EndCombo();
            }
            return ret;
        }

        public bool Combo(string label, ref string selectedId) => Combo(label, selectedId, ref selectedId);

        #endregion
    }
}
