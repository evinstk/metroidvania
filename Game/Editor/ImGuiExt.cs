using ImGuiNET;
using Nez;
using System.Reflection;

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
            var layerFields = typeof(PhysicsLayer).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in layerFields)
            {
                ImGui.PushID(field.Name);
                var value = (int)field.GetValue(null);
                var isSet = Flags.IsUnshiftedFlagSet(mask, value);
                if (ImGui.Checkbox(field.Name, ref isSet))
                    SetFlagValue(ref mask, value, isSet);
                ImGui.PopID();
            }
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
