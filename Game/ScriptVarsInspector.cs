using ImGuiNET;
using Nez;
using Nez.ImGuiTools;

namespace Game
{
    class ScriptVarsInspector : Component
    {
        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            var vars = Entity.GetMainScene().GetScriptVars();

            if (ImGui.Begin("Script Vars"))
            {
                var bools = vars.GetAll<bool>();
                foreach (var b in bools)
                {
                    var val = (bool)b.Value;
                    if (ImGui.Checkbox(b.Key, ref val))
                        vars[b.Key] = val;
                }
                ImGui.End();
            }
        }
    }
}
