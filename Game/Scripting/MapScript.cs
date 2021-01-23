﻿using Game.Editor;
using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using Nez;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Scripting
{
    struct Trigger
    {
        public Closure Condition;
        public Closure Effect;
        public string RoomId;
    }

    class ScriptException : Exception
    {
        public ScriptException(Exception inner)
            : base("An error occurred whie running a script.", inner)
        {
        }
    }

    class RoomVariables
    {
        Dictionary<string, object> _vars = new Dictionary<string, object>();

        public RoomVariables(List<RoomVariable> vars)
        {
            foreach (var variable in vars)
            {
                _vars[variable.Name] = variable.Value;
            }
        }

        public object this[string key]
        {
            get
            {
                if (!_vars.TryGetValue(key, out var val))
                    return null;
                return val;
            }
            set { _vars[key] = value; }
        }
    }

    class ScriptableObjects
    {
        struct GeneralManager
        {
            public Manager Manager;
            public Type Type;
        }
        List<GeneralManager> _soManagers = new List<GeneralManager>();

        public ScriptableObjects()
        {
            var soTypes = ReflectionUtils.GetAllSubclasses(typeof(ScriptableObject));
            foreach (var soType in soTypes)
            {
                var managerType = typeof(ScriptableObjectManager<>).MakeGenericType(soType);
                var manager = (Manager)typeof(Core).GetMethod(nameof(Core.GetGlobalManager)).MakeGenericMethod(managerType).Invoke(null, null);
                _soManagers.Add(new GeneralManager
                {
                    Manager = manager,
                    Type = soType,
                });
            }
        }

        public object this[string key]
        {
            get
            {
                foreach (var gm in _soManagers)
                {
                    var obj = typeof(Manager<>)
                        .MakeGenericType(gm.Type)
                        .GetMethod("GetResourceByName")
                        .Invoke(gm.Manager, new object[] { key });
                    if (obj != null)
                        return obj;
                }
                return null;
            }
        }
    }

    class MapScript : Component, IUpdatable
    {
        struct QueuedScript
        {
            public string RoomId;
            public string ScriptCode;
            public List<RoomVariable> Variables;
        }

        List<QueuedScript> _pendingScripts = new List<QueuedScript>();
        Dictionary<string, Script> _scripts = new Dictionary<string, Script>();

        List<Trigger> _pendingAdditions = new List<Trigger>();
        List<Trigger> _triggers = new List<Trigger>();
        List<Trigger> _pendingRemovals = new List<Trigger>();

        List<DynValue> _pendingCoroutineAdditions = new List<DynValue>();
        List<DynValue> _coroutines = new List<DynValue>();
        List<DynValue> _pendingCoroutineRemovals = new List<DynValue>();

        string _commonCode;

        VirtualButton _interactInput;

        DialogSystem _dialogSystem;

        static MapScript()
        {
            UserData.RegisterProxyType<EntityProxy, Entity>(e => new EntityProxy(e));
            UserData.RegisterType<RoomVariables>();
            UserData.RegisterType<ScriptableObjects>();
            var soTypes = ReflectionUtils.GetAllSubclasses(typeof(ScriptableObject));
            foreach (var soType in soTypes)
                UserData.RegisterType(soType);
            UserData.RegisterType<ScriptEntity>();

            Script.DefaultOptions.DebugPrint = s => Debug.Log(s);
        }

        public override void OnAddedToEntity()
        {
            _dialogSystem = Entity.Scene.FindComponentOfType<DialogSystem>();
            Insist.IsNotNull(_dialogSystem);

            _interactInput = new VirtualButton();
            _interactInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));
            _interactInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Y));

            _commonCode = File.ReadAllText(ContentPath.Scripts + "common.lua");
        }

        public void Queue(string roomId, string scriptSrc, List<RoomVariable> roomVariables = null)
        {
            _pendingScripts.Add(new QueuedScript
            {
                RoomId = roomId,
                ScriptCode = File.ReadAllText(ContentPath.Scripts + scriptSrc),
                Variables = roomVariables,
            });
        }

        void LoadScript(QueuedScript payload)
        {
            if (!_scripts.TryGetValue(payload.RoomId, out var script))
            {
                script = new Script();
                script.Globals["trigger"] = (Action<Closure, Closure>)((Closure condition, Closure effect) =>
                {
                    _pendingAdditions.Add(new Trigger()
                    {
                        Condition = condition,
                        Effect = effect,
                        RoomId = payload.RoomId,
                    });
                });
                script.Globals["findEntity"] = (Func<string, Entity>)FindEntity;
                script.Globals["find_entity"] = (Func<string, ScriptEntity>)FindScriptEntity;
                script.Globals["speak"] = (Action<string>)_dialogSystem.FeedLine;
                script.Globals["interact"] = (Func<bool>)Interact;
                script.Globals["instantiate"] = (Func<string, int, int, ScriptEntity>)Instantiate;
                script.Globals["vars"] = new ScriptableObjects();
                script.Globals["start_coroutine"] = (Action<Closure>)((Closure fn) =>
                {
                    var coroutine = _scripts[payload.RoomId].CreateCoroutine(fn);
                    _pendingCoroutineAdditions.Add(coroutine);
                });

                script.DoString(_commonCode);

                if (payload.Variables != null)
                    script.Globals["roomVars"] = new RoomVariables(payload.Variables);

                _scripts.Add(payload.RoomId, script);
            }

            script.DoString(payload.ScriptCode);
        }

        public void Update()
        {
            try
            {
                foreach (var script in _pendingScripts)
                {
                    LoadScript(script);
                }
                _pendingScripts.Clear();

                foreach (var addition in _pendingAdditions)
                {
                    _triggers.Add(addition);
                }
                _pendingAdditions.Clear();

                foreach (var removal in _pendingRemovals)
                {
                    _triggers.Remove(removal);
                }
                _pendingRemovals.Clear();

                foreach (var trigger in _triggers)
                {
                    if (trigger.Condition.Call().Boolean)
                    {
                        var coroutine = _scripts[trigger.RoomId].CreateCoroutine(trigger.Effect);
                        _coroutines.Add(coroutine);
                        _pendingRemovals.Add(trigger);
                    }
                }

                foreach (var pendingCoroutine in _pendingCoroutineAdditions)
                {
                    _coroutines.Add(pendingCoroutine);
                }
                _pendingCoroutineAdditions.Clear();
                foreach (var coroutine in _coroutines)
                {
                    coroutine.Coroutine.Resume();
                    if (coroutine.Coroutine.State == CoroutineState.Dead)
                        _pendingCoroutineRemovals.Add(coroutine);
                }
                foreach (var coroutineRemoval in _pendingCoroutineRemovals)
                {
                    _coroutines.Remove(coroutineRemoval);
                }
                _pendingCoroutineRemovals.Clear();

                _interactionConsumed = false;
            }
            catch (Exception exception)
            {
                _pendingScripts.Clear();
                _pendingAdditions.Clear();
                _pendingRemovals.Clear();
                _triggers.Clear();
                _coroutines.Clear();
                _pendingCoroutineRemovals.Clear();
                throw new ScriptException(exception);
            }
        }

        Entity FindEntity(string name)
        {
            var entity = Core.Scene.FindEntity(name);
            Debug.LogIf(entity == null, $"No entity with name \"{name}\"");
            return entity;
        }

        ScriptEntity FindScriptEntity(string name)
        {
            var entity = Core.Scene.FindEntity(name);
            Debug.LogIf(entity == null, $"No entity with name \"{name}\"");
            return new ScriptEntity(entity);
        }

        // can only call interact() once per frame
        bool _interactionConsumed = false;
        bool Interact()
        {
            if (!_interactionConsumed)
            {
                _interactionConsumed = true;
                return _interactInput.IsPressed;
            }
            return false;
        }

        ScriptEntity Instantiate(string name, int x, int y)
        {
            var prefab = Core.GetGlobalManager<PrefabManager>().GetResourceByName(name);
            if (prefab == null)
            {
                Debug.Log($"No prefab name {name}");
                return null;
            }
            var roomEntity = new RoomEntity
            {
                Name = prefab.Name,
                PrefabId = prefab.Id,
                Position = new Microsoft.Xna.Framework.Vector2(x, y),
            };
            var entity = roomEntity.CreateEntity(Entity.Scene);
            return new ScriptEntity(entity);
        }

    }

    class EntityProxy
    {
        Entity _target;

        public EntityProxy(Entity target)
        {
            _target = target;
        }
    }
}
