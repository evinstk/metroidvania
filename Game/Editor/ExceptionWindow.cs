using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;

namespace Game.Editor
{
    [EditorWindow]
    class ExceptionWindow : Component
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
            if (EditorState.Exception == null) return;

            if (ImGui.Begin("Exception"))
            {
                ImGui.TextColored(Color.Red.ToVector4().ToNumerics(), EditorState.Exception.Message);
                ImGui.Text(EditorState.Exception.InnerException.Message);
                ImGui.Text(EditorState.Exception.InnerException.StackTrace);
                ImGui.End();
            }
        }
    }
}
