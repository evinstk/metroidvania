using Game.Editor;
using Game.Editor.Scriptable;
using Nez;
using Nez.Console;
using System;
using System.Collections.Generic;

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
            IsMouseVisible = false;

            var managers = new List<Manager>();
            var managerTypes = ReflectionUtils.GetAllSubclasses(typeof(Manager), true);
            foreach (var managerType in managerTypes)
            {
                if (!managerType.ContainsGenericParameters)
                {
                    var manager = Activator.CreateInstance(managerType) as Manager;
                    RegisterGlobalManager(manager);
                    managers.Add(manager);
                }
            }

            var soTypes = ReflectionUtils.GetAllSubclasses(typeof(ScriptableObject));
            foreach (var soType in soTypes)
            {
                var manager = Activator.CreateInstance(typeof(ScriptableObjectManager<>).MakeGenericType(soType)) as Manager;
                RegisterGlobalManager(manager);
                managers.Add(manager);
            }

            foreach (var manager in managers)
                manager.Initialize();

#if RELEASE
            Screen.IsFullscreen = true;
#endif
            Screen.SetSize(MainScene.ScreenWidth, MainScene.ScreenHeight);

            Scene = new MainMenuScene();
        }
    }
}
