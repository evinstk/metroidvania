using Nez;
using System;

namespace Game.Editor.World
{
    static class WorldEditorState
    {
        public static string WorldId
        {
            get { return _worldId; }
            set
            {
                _worldId = value;
                OnSetWorld?.Invoke();
            }
        }
        static string _worldId;
        public static World World => Core.GetGlobalManager<WorldManager>().GetResource(_worldId);
        public static event Action OnSetWorld;
    }
}
