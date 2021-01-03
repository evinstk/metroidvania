using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System.Collections.Generic;

namespace Game.Editor.RoomEdge
{
    class RoomEdge : IResource
    {
        [JsonInclude]
        public string Id { get; set; } = Utils.RandomString();
        public string DisplayName => "Some link";

        public List<RoomEdgeNode> Rooms = new List<RoomEdgeNode>
        {
            new RoomEdgeNode(),
            new RoomEdgeNode(),
        };
    }

    class RoomEdgeNode
    {
        public string RoomId = "";
        public Point Position;
    }

    class RoomEdgeManager : Manager<RoomEdge>
    {
        public override string Path => ContentPath.RoomEdges;

        public List<RoomEdge> GetEdges(string roomId)
        {
            var edges = new List<RoomEdge>();
            foreach (var edge in _resources)
            {
                if (edge.Data.Rooms.Find(r => r.RoomId == roomId) != null)
                    edges.Add(edge.Data);
            }
            return edges;
        }
    }
}
