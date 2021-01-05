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
    }

    class MapScript : Component, IUpdatable
    {
        static string _commonCode = File.ReadAllText(ContentPath.Scripts + "common.lua");

        string _scriptSrc;

        List<Trigger> _pendingAdditions = new List<Trigger>();
        List<Trigger> _triggers = new List<Trigger>();
        List<Trigger> _pendingRemovals = new List<Trigger>();
        List<DynValue> _coroutines = new List<DynValue>();
        List<DynValue> _pendingCoroutineRemovals = new List<DynValue>();

        Script _script;

        static MapScript()
        {
            UserData.RegisterProxyType<EntityProxy, Entity>(e => new EntityProxy(e));
        }

        public MapScript() { }

        public MapScript(string scriptSrc)
        {
            _scriptSrc = scriptSrc;
        }

        public override void OnAddedToEntity()
        {
            _script = new Script();
            _script.Globals["trigger"] = (Action<Closure, Closure>)Trigger;
            _script.Globals["findEntity"] = (Func<string, Entity>)FindEntity;
            _script.Globals["move"] = (Action<Entity, Entity>)Move;
            _script.Globals["stop"] = (Action<Entity>)Stop;
            _script.Globals["collides"] = (Func<Entity, Entity, bool>)Collides;
            _script.Globals["destroy"] = (Action<Entity>)Destroy;

            //var scene = Entity.Scene as MainScene;
            //script.Globals["spawn"] = scene.Spawn;

            _script.DoString(_commonCode);
            if (_scriptSrc != null)
            {
                var scriptCode = File.ReadAllText(ContentPath.Scripts + _scriptSrc);
                _script.DoString(scriptCode);
            }
        }

        public void Update()
        {
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
                    var coroutine = _script.CreateCoroutine(trigger.Effect);
                    _coroutines.Add(coroutine);
                    _pendingRemovals.Add(trigger);
                }
            }

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
        }

        void Trigger(Closure condition, Closure effect)
        {
            _pendingAdditions.Add(new Trigger()
            {
                Condition = condition,
                Effect = effect,
            });
        }

        Entity FindEntity(string name)
        {
            var entity = Core.Scene.FindEntity(name);
            Debug.LogIf(entity == null, $"No entity with name \"{name}\"");
            return entity;
        }

        void Move(Entity entity, Entity dest)
        {
            var entityNull = entity == null; var destNull = dest == null;
            Debug.LogIf(entityNull, "Argument entity not defined");
            Debug.LogIf(destNull, "Argument dest not defined");
            if (entityNull || destNull) return;

            var controller = entity.GetComponent<FreeController>();
            if (controller == null)
            {
                Debug.Log($"No FreeController found on \"{entity.Name}\"");
                return;
            }
            controller.SetXAxis(Mathf.SignThreshold(dest.Position.X - entity.Position.X, 0));
            Debug.Log($"Moving \"{entity.Name}\" to \"{dest.Name}\"");
        }

        void Stop(Entity entity)
        {
            var entityNull = entity == null;
            Debug.LogIf(entityNull, "Argument entity not defined");
            if (entityNull) return;

            var controller = entity.GetComponent<FreeController>();
            if (controller == null)
            {
                Debug.Log($"No FreeController found on \"{entity.Name}\"");
                return;
            }
            controller.SetXAxis(0);
            Debug.Log($"Stopping \"{entity.Name}\"");
        }

        void Destroy(Entity entity)
        {
            if (entity == null)
            {
                Debug.Log("Argument entity not defined");
                return;
            }
            entity.Destroy();
            Debug.Log($"Destroying \"{entity.Name}\"");
        }

        bool Collides(Entity lhs, Entity rhs)
        {
            var lhsNull = lhs == null; var rhsNull = rhs == null;
            Debug.LogIf(lhsNull, "Argument lhs not defined");
            Debug.LogIf(rhsNull, "Argument rhs not defined");
            if (lhsNull || rhsNull) return false;

            var colliderL = lhs.GetComponent<Collider>();
            var colliderR = rhs.GetComponent<Collider>();
            var colliderLNull = colliderL == null; var colliderRNull = colliderR == null;
            Debug.LogIf(colliderLNull, "No collider on argument \"lhs\"");
            Debug.LogIf(colliderRNull, "No collider on argument \"rhs\"");
            if (colliderLNull || colliderRNull) return false;

            return colliderL.CollidesWith(colliderR, out _);
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
