using Nez;
using System;
using System.Collections.Generic;

namespace Game
{
    class ScriptVars
    {
        Dictionary<string, object> _vars = new Dictionary<string, object>();

        public T Get<T>(string varName)
        {
            if (_vars.TryGetValue(varName, out var val))
            {
                if (val is T tVal)
                    return tVal;
                else
                    throw new Exception($"Variable {varName} previously used for different type.");
            }

            var newVal = (T)Activator.CreateInstance(typeof(T));
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
    }

    static class ScriptVarsExt
    {
        public static ScriptVars GetScriptVars(this Scene scene) => (scene as MainScene).ScriptVars;
    }
}
