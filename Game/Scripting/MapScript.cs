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
        static string _commonCode = File.ReadAllText("Content/Scripts/common.lua");

        string _scriptSrc;

        List<Trigger> _pendingAdditions = new List<Trigger>();
        List<Trigger> _triggers = new List<Trigger>();
        List<Trigger> _pendingRemovals = new List<Trigger>();

        static MapScript()
        {
            UserData.RegisterProxyType<EntityProxy, Entity>(e => new EntityProxy(e));
        }

        public MapScript(string scriptSrc)
        {
            _scriptSrc = scriptSrc;
        }

        public override void OnAddedToEntity()
        {
            var script = new Script();
            script.Globals["trigger"] = (Action<Closure, Closure>)Trigger;
            script.Globals["findEntity"] = (Func<string, Entity>)FindEntity;
            script.Globals["move"] = (Action<Entity, Entity>)Move;
            script.Globals["collides"] = (Func<Entity, Entity, bool>)Collides;
            script.Globals["destroy"] = (Action<Entity>)Destroy;

            var scene = Entity.Scene as MainScene;
            script.Globals["spawn"] = scene.Spawn;

            var scriptCode = File.ReadAllText("Content/Scripts/" + _scriptSrc);
            script.DoString(_commonCode);
            script.DoString(scriptCode);
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
                    trigger.Effect.Call();
                    _pendingRemovals.Add(trigger);
                }
            }
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
