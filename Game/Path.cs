namespace Game
{
    static class ContentPath
    {
        public static readonly string Textures = "Content/Textures/";

#if DEBUG
        public static readonly string Prefabs = "../../../Content/Prefabs/";
        public static readonly string Rooms = "../../../Content/Rooms/";
        public static readonly string Animations = "../../../Content/Animations2/";
        public static readonly string RoomEdges = "../../../Content/RoomEdges/";
        public static readonly string Scripts = "../../../Content/Scripts/";
#else
        public static readonly string Prefabs = "Content/Prefabs/";
        public static readonly string Rooms = "Content/Rooms/";
        public static readonly string Animations = "Content/Animations2/";
        public static readonly string RoomEdges = "Content/RoomEdges/";
        public static readonly string Scripts = "Content/Scripts/";
#endif
    }
}
