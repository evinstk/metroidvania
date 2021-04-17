using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using Nez;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Game.Scripts
{
    class ScriptLoader : SceneComponent
    {
        static readonly string[] _modulePaths = new string[]
        {
            $"{ContentPath.Root}?.lua",
            $"{ContentPath.Root}?",
        };

        static ScreenProxy _screenProxy = new ScreenProxy();
        static Dictionary<string, int> _masks = new Dictionary<string, int>();

        static ScriptLoader()
        {
            UserData.RegisterType<ScreenProxy>();
            UserData.RegisterType<Vector2>();
            UserData.RegisterProxyType<PrototypeSceneProxy, Scene>(s => new PrototypeSceneProxy(s));
            Script.DefaultOptions.DebugPrint = s => Debug.Log(s);

            var masks = typeof(Mask).GetFields(BindingFlags.Public|BindingFlags.Static);
            foreach (var mask in masks)
                _masks[$"MASK_{mask.Name.ToUpper()}"] = (int)mask.GetValue(null);
        }

        public Script LoadScript(string scriptFile)
        {
            var scriptCode = File.ReadAllText($"{ContentPath.Root}{scriptFile}");
            var script = new Script();
            ((ScriptLoaderBase)script.Options.ScriptLoader).ModulePaths = _modulePaths;

            script.Globals["vec2"] = (Func<float, float, Vector2>)Vec2;
            foreach (var mask in _masks)
                script.Globals[mask.Key] = mask.Value;
            script.Globals["screen"] = _screenProxy;
            script.Globals["scene"] = Scene;

            script.DoString(scriptCode);
            return script;
        }

        Vector2 Vec2(float x, float y) => new Vector2(x, y);
    }
}
