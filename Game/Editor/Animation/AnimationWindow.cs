using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Num = System.Numerics;

namespace Game.Editor.Animation
{
    [EditorWindow]
    class AnimationWindow : Component
    {
        AnimationManager _animationManager;
        TextureMapDataManager _textureMapDataManager;

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(Draw);

            _animationManager = Core.GetGlobalManager<AnimationManager>();
            _textureMapDataManager = Core.GetGlobalManager<TextureMapDataManager>();
        }

        public override void OnRemovedFromEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().UnregisterDrawCommand(Draw);
        }

        void Draw()
        {
            if (ImGui.Begin("Animation", ImGuiWindowFlags.MenuBar))
            {
                DrawMenuBar();

                ImGui.BeginChild("Animations", new Num.Vector2(200, 0), true);
                DrawLeftPane();
                ImGui.EndChild();

                ImGui.SameLine();

                ImGui.BeginChild("Animation Inspector");
                DrawRightPane();
                ImGui.EndChild();

                ImGui.End();
            }
        }

        void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save All"))
                        _animationManager.SaveAll();
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
        }

        void DrawLeftPane()
        {
            var newAnimation = false;

            _animationManager.RadioButtons(EditorState.SelectedAnimationId, ref EditorState.SelectedAnimationId);
            if (NezImGui.CenteredButton("New Animation", 1f))
            {
                newAnimation = true;
            }

            if (newAnimation)
                ImGui.OpenPopup("new-resource");

            _animationManager.NewResourcePopup();
        }

        void DrawRightPane()
        {
            var animation = _animationManager.GetResource(EditorState.SelectedAnimationId);
            if (animation != null)
            {
                AnimationData.Frame deleteFrame = null;

                ImGui.InputText("##name", ref animation.Name, 25);
                ImGui.SameLine();
                ImGui.Text("Name");

                ImGui.InputInt("##fps", ref animation.FramesPerSecond);
                ImGui.SameLine();
                ImGui.Text("FPS");

                for (var i = 0; i < animation.Frames.Count; ++i)
                {
                    ImGui.PushID(i);
                    var frame = animation.Frames[i];

                    ImGui.Text($"Frame {i}:");

                    _textureMapDataManager.Combo("Texture Map", frame.Sprite.TextureMapId, ref frame.Sprite.TextureMapId);
                    var textureMap = _textureMapDataManager.GetResource(frame.Sprite.TextureMapId);
                    if (textureMap != null)
                    {
                        if (ImGui.BeginCombo("Frame", frame.Sprite.FrameFilename))
                        {
                            foreach (var spriteFrame in textureMap.frames)
                            {
                                if (ImGui.Selectable(spriteFrame.filename, spriteFrame.filename == frame.Sprite.FrameFilename))
                                {
                                    frame.Sprite.FrameFilename = spriteFrame.filename;
                                }
                            }
                            ImGui.EndCombo();
                        }
                    }
                    if (ImGui.Button("Delete Frame"))
                    {
                        deleteFrame = frame;
                    }
                    ImGui.PopID();
                }
                if (NezImGui.CenteredButton("Add Frame", 1f))
                {
                    animation.Frames.Add(new AnimationData.Frame());
                }
                if (deleteFrame != null)
                {
                    animation.Frames.Remove(deleteFrame);
                }
            }
        }
    }
}
