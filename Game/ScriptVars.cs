using Nez;
using System;
using System.Collections.Generic;

namespace Game
{
    class ScriptVars
    {
        Dictionary<string, object> _vars = new Dictionary<string, object>();

        // avoiding allocation
        Dictionary<Type, List<KeyValuePair<string, object>>> _byType = new Dictionary<Type, List<KeyValuePair<string, object>>>();

        public T Get<T>(string varName)
        {
            if (_vars.TryGetValue(varName, out var val))
            {
                if (val is T tVal)
                    return tVal;
                else
                    throw new Exception($"Variable {varName} previously used for different type.");
            }

            var newVal = typeof(T) == typeof(string) ? (T)(object)string.Empty : (T)Activator.CreateInstance(typeof(T));
            _vars[varName] = newVal;
            return newVal;
        }

        public void Set<T>(string varName, T val)
        {
            _vars[varName] = val;
        }

        public object this[string varName]
        {
            get
            {
                return _vars.ContainsKey(varName) ? _vars[varName] : null;
            }
            set
            {
                _vars[varName] = value;
            }
        }

        public List<KeyValuePair<string, object>> GetAll<T>()
        {
            var type = typeof(T);
            if (!_byType.TryGetValue(type, out var tempList))
            {
                tempList = new List<KeyValuePair<string, object>>();
                _byType[type] = tempList;
            }

            tempList.Clear();
            foreach (var obj in _vars)
            {
                if (obj.Value.GetType() == type)
                    tempList.Add(obj);
            }

            return tempList;
        }
    }

    static class ScriptVarsExt
    {
        public static ScriptVars GetScriptVars(this Scene scene) => (scene as MainScene).ScriptVars;
    }
}
