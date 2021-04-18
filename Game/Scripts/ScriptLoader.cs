using Game.Audio;
using Game.Prototypes;
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

        List<Script> _scripts = new List<Script>();
        List<DynValue> _pendingCoroutines = new List<DynValue>();
        List<DynValue> _coroutines = new List<DynValue>();
        List<DynValue> _coroutineRemovals = new List<DynValue>();

        Dictionary<string, List<Closure>> _eventListeners = new Dictionary<string, List<Closure>>();

        static ScriptLoader()
        {
            UserData.RegisterType<ScreenProxy>();
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<SceneSettings>();
            UserData.RegisterType<FMOD.Studio.EventInstance>();
            UserData.RegisterProxyType<AudioManagerProxy, AudioManager>(m => new AudioManagerProxy(m));
            UserData.RegisterProxyType<PrototypeSceneProxy, Scene>(s => new PrototypeSceneProxy(s));
            UserData.RegisterProxyType<EntityProxy, Entity>(e => new EntityProxy(e));
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
            script.Globals["audio"] = Core.GetGlobalManager<AudioManager>();
            script.Globals["scene"] = Scene;
            script.Globals["scene_settings"] = Scene.GetSceneComponent<SceneSettings>();
            script.Globals["on_event"] = (Action<string, Closure>)OnEvent;
            script.Globals["start_coroutine"] = (Action<Closure>)((Closure fn) =>
            {
                var coroutine = script.CreateCoroutine(fn);
                _pendingCoroutines.Add(coroutine);
            });

            script.DoString(scriptCode);
            _scripts.Add(script);
            return script;
        }

        public override void Update()
        {
            foreach (var script in _scripts)
                script.Globals["delta_time"] = Time.DeltaTime;
            foreach (var pending in _pendingCoroutines)
                _coroutines.Add(pending);
            _pendingCoroutines.Clear();
            foreach (var coroutine in _coroutines)
            {
                coroutine.Coroutine.Resume();
                if (coroutine.Coroutine.State == CoroutineState.Dead)
                    _coroutineRemovals.Add(coroutine);
            }
            foreach (var removal in _coroutineRemovals)
                _coroutines.Remove(removal);
            _coroutineRemovals.Clear();
        }

        public void RaiseEvent<T>(string eventName, T payload)
        {
            if (_eventListeners.TryGetValue(eventName, out var listeners))
            {
                foreach (var listener in listeners)
                    listener.Call(payload);
            }
        }

        void OnEvent(string eventName, Closure fn)
        {
            if (_eventListeners.TryGetValue(eventName, out var listeners))
            {
                listeners.Add(fn);
            }
            _eventListeners.Add(eventName, new List<Closure> { fn });
        }

        static Vector2 Vec2(float x, float y) => new Vector2(x, y);
    }
}
