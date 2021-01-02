using Game.Editor.Prefab;
using Game.Editor.Tool;
using Nez;

namespace Game.Editor
{
    static class EditorState
    {
        public static RoomData RoomData => Core.GetGlobalManager<RoomManager>().GetResource(SelectedRoomId);
        public static string SelectedRoomId = null;

        public static PrefabData SelectedPrefab => Core.GetGlobalManager<PrefabManager>().GetResource(SelectedPrefabId);
        public static string SelectedPrefabId = null;

        public static Tools CurrentTool { get; set; } = Tools.Brush;

        public static string SelectedAnimationId = null;
    }
}
