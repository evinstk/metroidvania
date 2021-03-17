using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using Nez;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Scripting
{
    class SceneScript
    {
        DialogSystem _dialogSystem;
        ScriptVars _scriptVars;

        List<Script> _scripts = new List<Script>();
        List<DynValue> _pendingCoroutines = new List<DynValue>();
        List<DynValue> _coroutines = new List<DynValue>();
        List<DynValue> _coroutineRemovals = new List<DynValue>();

        VirtualButton _inputInteract;

        static SceneScript()
        {
            UserData.RegisterType<ScriptVars>();
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<ScriptData>();
            UserData.RegisterProxyType<ChestContentsProxy, ChestContents>(c => new ChestContentsProxy(c));
            UserData.RegisterProxyType<SceneProxy, MainScene>(s => new SceneProxy(s));
            UserData.RegisterProxyType<CameraProxy, Camera>(c => new CameraProxy(c));
            UserData.RegisterProxyType<EntityProxy, Entity>(e => new EntityProxy(e));
            Script.DefaultOptions.DebugPrint = s => Debug.Log(s);
        }

        static readonly string[] _modulePaths = new string[]
        {
            $"{ContentPath.Scripts}?.lua",
            $"{ContentPath.Scripts}?",
        };

        public SceneScript(
            DialogSystem dialogSystem,
            ScriptVars scriptVars)
        {
            _dialogSystem = dialogSystem;
            _scriptVars = scriptVars;

            _inputInteract = new VirtualButton();
            _inputInteract.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Y));
            _inputInteract.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));

            var dataScript = new Script();
            ((ScriptLoaderBase)dataScript.Options.ScriptLoader).ModulePaths = _modulePaths;
            var scriptCode = File.ReadAllText($"{ContentPath.Scripts}data.lua");
            dataScript.Globals["vars"] = _scriptVars;
            dataScript.Globals["data"] = new ScriptData();
            dataScript.DoString(scriptCode);
        }

        public void LoadScript(string scriptFile)
        {
            var scriptCode = File.ReadAllText($"{ContentPath.Scripts}{scriptFile}");

            var script = new Script();

            ((ScriptLoaderBase)script.Options.ScriptLoader).ModulePaths = _modulePaths;
            script.Globals["line"] = (Action<string, string, string, List<string>, int?, int?, bool?, float?, bool?>)((line, portrait, speaker, options, x, y, showBorder, pitch, playSound) =>
            {
                _dialogSystem.FeedLine(line, portrait, speaker, options, new Vector2(x ?? 10, y ?? 10), showBorder ?? true, pitch ?? 0f, playSound ?? true);
            });
            script.Globals["read_dialog_option"] = (Func<int>)(() => _dialogSystem.OptionIndex + 1);
            script.Globals["vars"] = _scriptVars;
            script.Globals["start_coroutine"] = (Action<Closure>)((Closure fn) =>
            {
                var coroutine = script.CreateCoroutine(fn);
                _pendingCoroutines.Add(coroutine);
            });
            script.Globals["scene"] = (MainScene)Core.Scene;
            script.Globals["camera"] = Core.Scene.Camera;

            script.DoString(scriptCode);

            _scripts.Add(script);
        }

        public void Update()
        {
            foreach (var script in _scripts)
            {
                script.Globals["delta_time"] = Time.DeltaTime;
                script.Globals["interaction_pressed"] = _inputInteract.IsPressed;
            }
            foreach (var pendingCoroutine in _pendingCoroutines)
                _coroutines.Add(pendingCoroutine);
            _pendingCoroutines.Clear();
            foreach (var coroutine in _coroutines)
            {
                coroutine.Coroutine.Resume();
                if (coroutine.Coroutine.State == CoroutineState.Dead)
                    _coroutineRemovals.Add(coroutine);
            }
            foreach (var coroutineRemoval in _coroutineRemovals)
                _coroutines.Remove(coroutineRemoval);
            _coroutineRemovals.Clear();
        }
    }
}
