using Nez;
using Nez.Tiled;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    interface ICondition
    {
        bool Check();
    }

    class OnEnter : ICondition
    {
        public string EntityName;
        public string Region;

        public bool Check()
        {
            var entity = Core.Scene.FindEntity(EntityName).GetComponent<Collider>();
            var region = Core.Scene.FindEntity(Region).GetComponent<Collider>();
            return entity.CollidesWith(region, out _);
        }
    }

    interface IScript
    {
        IEnumerable<ICondition> GetSteps();
    }

    class NullScript : IScript
    {
        public IEnumerable<ICondition> GetSteps() => Enumerable.Empty<ICondition>();
    }

    class LeaveScript : IScript
    {
        public string EntityName;
        public string LeaveZone;

        public IEnumerable<ICondition> GetSteps()
        {
            var entity = Core.Scene.FindEntity(EntityName);
            var entityController = entity.GetComponent<FreeController>();
            var leaveZone = Core.Scene.FindEntity(LeaveZone);

            entityController.SetXAxis(Mathf.SignThreshold(leaveZone.Position.X - entity.Position.X, 0));
            var onEnter = new OnEnter();
            onEnter.EntityName = EntityName;
            onEnter.Region = LeaveZone;
            yield return onEnter;
            entity.Destroy();
        }
    }

    class ScriptExecution
    {
        IEnumerator<ICondition> _steps;

        public ScriptExecution(IScript script)
        {
            _steps = script.GetSteps().GetEnumerator();
        }

        public bool Update()
        {
            if (_steps.Current?.Check() == false)
            {
                return false;
            }
            return !_steps.MoveNext();
        }
    }

    class ScriptComponent : Component, IUpdatable
    {
        List<(ICondition Condition, IScript Script)> _scripts = new List<(ICondition Condition, IScript Script)>();
        List<(ICondition Condition, IScript Script)> _pendingRemovals = new List<(ICondition Condition, IScript Script)>();
        List<ScriptExecution> _executions = new List<ScriptExecution>();
        List<ScriptExecution> _terminatingExecutions = new List<ScriptExecution>();

        public void RegisterScript(TmxObject scriptObj)
        {
            ICondition condition = null;
            var conditionType = scriptObj.Properties["condition"];
            if (conditionType == "onEnter")
            {
                var onEnter = new OnEnter();
                onEnter.EntityName = scriptObj.Properties["condition:entityName"];
                onEnter.Region = "trigger" + scriptObj.Properties["condition:region"];
                condition = onEnter;
            }
            Insist.IsNotNull(condition);

            IScript script = null;
            var leaveType = scriptObj.Properties["script"];
            if (leaveType == "leave")
            {
                var leave = new LeaveScript();
                leave.EntityName = scriptObj.Properties["script:entityName"];
                leave.LeaveZone = "trigger" + scriptObj.Properties["script:leaveZone"];
                script = leave;
            }
            Insist.IsNotNull(script);

            _scripts.Add((condition, script));
        }

        public void Update()
        {
            foreach (var s in _scripts)
            {
                if (s.Condition.Check())
                {
                    _executions.Add(new ScriptExecution(s.Script));
                    _pendingRemovals.Add(s);
                }
            }

            foreach (var removal in _pendingRemovals)
            {
                _scripts.Remove(removal);
            }
            _pendingRemovals.Clear();

            foreach (var execution in _executions)
            {
                if (execution.Update())
                {
                    _terminatingExecutions.Add(execution);
                }
            }

            foreach (var removal in _terminatingExecutions)
            {
                _executions.Remove(removal);
            }
            _terminatingExecutions.Clear();
        }
    }
}
