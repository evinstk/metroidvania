using Game.Editor;
using MoonSharp.Interpreter;
using Nez;
using System;
using System.IO;

namespace Game.Scripting
{
    class EntityScript : Component, IUpdatable
    {
        DynValue _updateFn;

        public override void OnAddedToEntity()
        {
            var scripting = Entity.Scene.FindComponentOfType<MapScript>();
            var roomEntity = Entity.GetComponentStrict<RoomEntityComponent>();

            var script = scripting.GetScript(roomEntity.RoomId);
            var code = File.ReadAllText(ContentPath.ComponentScripts + "switch.lua");
            try
            {
                _updateFn = script.CreateCoroutine(script.DoString(code).Function);
            }
            catch (Exception e)
            {
                _updateFn = null;
                throw new ScriptException(e);
            }
        }

        public void Update()
        {
            try
            {
                if (_updateFn != null)
                {
                    _updateFn.Coroutine.Resume();
                    if (_updateFn.Coroutine.State == CoroutineState.Dead)
                        _updateFn = null;
                }
            }
            catch (Exception e)
            {
                _updateFn = null;
                throw new ScriptException(e);
            }
        }
    }
}
