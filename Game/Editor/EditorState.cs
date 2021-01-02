using Nez;

namespace Game.Editor
{
    static class EditorState
    {
        public static RoomData RoomData => Core.GetGlobalManager<RoomManager>().GetResource(SelectedRoomId);
        public static string SelectedRoomId = null;
    }
}
