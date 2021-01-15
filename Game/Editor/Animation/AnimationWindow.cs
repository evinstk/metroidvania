using ImGuiNET;
using Microsoft.Xna.Framework;
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

            _animationManager.RadioButtons(EditorState.SelectedAnimationId, ref EditorState.SelectedAnimationId, out _);
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

                ImGui.Separator();

                for (var i = 0; i < animation.Frames.Count; ++i)
                {
                    ImGui.PushID(i);
                    var frame = animation.Frames[i];

                    ImGui.Text($"Frame {i}:");

                    _textureMapDataManager.Combo("Texture Map", frame.Sprite.TextureMapId, ref frame.Sprite.TextureMapId);
                    _textureMapDataManager.FrameCombo(frame.Sprite.TextureMapId, ref frame.Sprite.FrameFilename);

                    var hitboxRemovalIndex = -1;
                    if (frame.HitBoxes.Count > 0)
                        ImGui.Text("Hitboxes:");
                    for (var h = 0; h < frame.HitBoxes.Count; ++h)
                    {
                        ImGui.PushID(h);
                        var hitbox = frame.HitBoxes[h];
                        var location = hitbox.Location;
                        ImGui.DragInt2("Location", ref location.X);
                        var size = hitbox.Size;
                        ImGui.DragInt2("Size", ref size.X);
                        frame.HitBoxes[h] = new Rectangle(location, size);
                        if (ImGui.Button("Remove Hitbox"))
                            hitboxRemovalIndex = h;
                        ImGui.Separator();
                        ImGui.PopID();
                    }
                    if (hitboxRemovalIndex > -1)
                        frame.HitBoxes.RemoveAt(hitboxRemovalIndex);

                    if (ImGui.Button("Add Hitbox"))
                    {
                        frame.HitBoxes.Add(new RectangleF());
                    }
                    if (ImGui.Button("Delete Frame"))
                    {
                        deleteFrame = frame;
                    }

                    ImGui.Separator();
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
