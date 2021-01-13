using Game.Editor.Scriptable;
using Game.Scripting;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Console;
using Nez.ImGuiTools;
using System;

namespace Game.Editor
{
    class EditorCore : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = true;
            DebugConsole.RenderScale = 2f;
            ExitOnEscapeKeypress = false;

            var imGuiManager = new ImGuiManager();
            RegisterGlobalManager(imGuiManager);

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

            Screen.SetSize(Screen.MonitorWidth, Screen.MonitorHeight);

            Scene = new RoomEditorScene();
        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                base.Update(gameTime);
            }
            catch (ScriptException exception)
            {
                EditorState.Exception = exception;
                Scene = new RoomEditorScene();
            }
        }
    }
}
