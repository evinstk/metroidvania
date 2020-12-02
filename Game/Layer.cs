using Nez;
using System;
using System.Linq;

namespace Game
{
    enum Teams
    {
        A,
        B,
    }

    static class Layer
    {
        public const int Default = 0;
        public const int Terrain = 1;

        const int TeamLayersOffset = 2;
        public static int GetTeamHurtBox(Teams team) => TeamLayersOffset + (int)team;
        public static int GetOtherTeamsMask(Teams team)
        {
            var mask = 0;
            var allTeams = Enum.GetValues(typeof(Teams)).Cast<int>();
            foreach (var t in allTeams)
            {
                if (t != (int)team)
                {
                    Flags.SetFlag(ref mask, t + TeamLayersOffset);
                }
            }
            return mask;
        }
    }
}
