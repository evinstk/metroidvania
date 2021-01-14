using Game.Editor;
using Game.Editor.Scriptable;
using Nez;
using Nez.Console;
using System;

namespace Game
{
    class RoomCore : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = true;
            DebugConsole.RenderScale = 2f;
            ExitOnEscapeKeypress = false;
            Window.Title = "Metroidvania";

            var managerTypes = ReflectionUtils.GetAllSubclasses(typeof(Manager), true);
            foreach (var managerType in managerTypes)
            {
                if (!managerType.ContainsGenericParameters)
                {
                    var manager = Activator.CreateInstance(managerType) as Manager;
                    RegisterGlobalManager(manager);
                }
            }

            var soTypes = ReflectionUtils.GetAllSubclasses(typeof(ScriptableObject));
            foreach (var soType in soTypes)
            {
                var manager = Activator.CreateInstance(typeof(ScriptableObjectManager<>).MakeGenericType(soType)) as Manager;
                RegisterGlobalManager(manager);
            }

            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

            Scene = new RoomScene();
        }
    }
}
