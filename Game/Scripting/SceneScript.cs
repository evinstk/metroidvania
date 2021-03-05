using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using Nez;
using Nez.Sprites;
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
            UserData.RegisterProxyType<ChestContentsProxy, ChestContents>(c => new ChestContentsProxy(c));
            UserData.RegisterProxyType<EntityProxy, Entity>(e => new EntityProxy(e));
            Script.DefaultOptions.DebugPrint = s => Debug.Log(s);
        }

        public SceneScript(
            DialogSystem dialogSystem,
            ScriptVars scriptVars)
        {
            _dialogSystem = dialogSystem;
            _scriptVars = scriptVars;

            _inputInteract = new VirtualButton();
            _inputInteract.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Y));
            _inputInteract.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));
        }

        public void LoadScript(string scriptFile)
        {
            var scriptCode = File.ReadAllText($"{ContentPath.Scripts}{scriptFile}");

            var script = new Script();

            ((ScriptLoaderBase)script.Options.ScriptLoader).ModulePaths = new string[]
            {
                $"{ContentPath.Scripts}?.lua",
                $"{ContentPath.Scripts}?",
            };
            script.Globals["line"] = (Action<string>)_dialogSystem.FeedLine;
            script.Globals["options"] = (Action<List<string>>)_dialogSystem.FeedOptions;
            script.Globals["portrait"] = (Action<string, SpriteAnimator.LoopMode>)_dialogSystem.ChangePortrait;
            script.Globals["read_dialog_option"] = (Func<int>)(() => _dialogSystem.OptionIndex + 1);
            script.Globals["vars"] = _scriptVars;
            script.Globals["find_entity"] = (Func<string, Entity>)FindEntity;
            script.Globals["start_coroutine"] = (Action<Closure>)((Closure fn) =>
            {
                var coroutine = script.CreateCoroutine(fn);
                _pendingCoroutines.Add(coroutine);
            });

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

        Entity FindEntity(string name) => Core.Scene.FindEntity(name);
    }
}
