using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System.Collections.Generic;

namespace Game.Editor.World
{
    class World : IResource
    {
        [NotInspectable]
        [JsonInclude]
        public string Id { get; set; } = Utils.RandomString();
        [JsonInclude]
        public string DisplayName { get; set; } = string.Empty;

        public List<WorldRoom> Rooms = new List<WorldRoom>();
    }

    class WorldRoom
    {
        [NotInspectable]
        public string Id = Utils.RandomString();
        public string Name;
        public string RoomId;
        public Point Position;

        public World World;
    }
}
