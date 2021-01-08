using ImGuiNET;
using Nez;

namespace Game.Editor
{
    static class ImGuiExt
    {
        public static void DrawPhysicsLayerInput(string label, ref int mask)
        {
            ImGui.InputInt(label, ref mask);
            ImGui.Indent();
            ImGui.Text("Team Bits:");
            for (var i = 7; i > -1; --i)
            {
                ImGui.PushID(label + i);
                bool isSet = Flags.IsUnshiftedFlagSet(mask, i);
                if (ImGui.Checkbox("", ref isSet))
                    SetFlagValue(ref mask, i, isSet);
                if (i > 0) ImGui.SameLine();
                ImGui.PopID();
            }
            var isTerrainSet = Flags.IsUnshiftedFlagSet(mask, RoomScene.PHYSICS_TERRAIN);
            ImGui.PushID(label + " terrain");
            if (ImGui.Checkbox("Terrain", ref isTerrainSet))
                SetFlagValue(ref mask, RoomScene.PHYSICS_TERRAIN, isTerrainSet);
            ImGui.PopID();
            ImGui.Unindent();
        }

        static void SetFlagValue(ref int mask, int flag, bool value)
        {
            if (value)
            {
                Flags.SetFlag(ref mask, flag);
            }
            else
            {
                Flags.UnsetFlag(ref mask, flag);
            }
        }
    }
}
