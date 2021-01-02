using Game.Editor.Animation;
using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Num = System.Numerics;

namespace Game.Editor.Prefab
{
    class PlayerMovementData : PrefabComponent
    {
        public AnimationData IdleRight = new AnimationData();
        public AnimationData IdleLeft = new AnimationData();
        public AnimationData WalkRight = new AnimationData();
        public AnimationData WalkLeft = new AnimationData();

        [CustomInspector(typeof(AnimationDataInspector))]
        public class AnimationData
        {
            public string AnimationId = "default";
            public bool Flip;
        }

        class AnimationDataInspector : AbstractTypeInspector
        {
            bool _isHeaderOpen;

            public override void DrawMutable()
            {
                var data = GetValue<AnimationData>();
                if (data == null) return;

                ImGui.Indent();
                NezImGui.BeginBorderedGroup();

                _isHeaderOpen = ImGui.CollapsingHeader($"{_name}");
                if (_isHeaderOpen)
                {
                    Core.GetGlobalManager<AnimationManager>().Combo("Animation", ref data.AnimationId);

                    ImGui.Checkbox("Flip", ref data.Flip);
                }

                NezImGui.EndBorderedGroup(new Num.Vector2(4, 1), new Num.Vector2(4, 2));
                ImGui.Unindent();
            }
        }
    }
}
