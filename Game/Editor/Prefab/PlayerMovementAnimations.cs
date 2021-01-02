using Game.Editor.Animation;
using Game.Movement;
using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Textures;
using System.Collections.Generic;
using System.IO;
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

        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<Animator<ObserverFrame>>();
            var movement = entity.AddComponent<PlayerMovement>();
            movement.Idle.Right = MakeAnimation(IdleRight);
            movement.Idle.Left = MakeAnimation(IdleLeft);
            movement.Walk.Right = MakeAnimation(WalkRight);
            movement.Walk.Left = MakeAnimation(WalkLeft);
        }

        Animation<ObserverFrame> MakeAnimation(AnimationData animationData)
        {
            var animationManager = Core.GetGlobalManager<AnimationManager>();
            var textureMapManager = Core.GetGlobalManager<TextureMapDataManager>();

            var animation = animationManager.GetResource(animationData.AnimationId);
            var frames = new List<ObserverFrame>();
            foreach (var frame in animation.Frames)
            {
                var textureMap = textureMapManager.GetResource(frame.Sprite.TextureMapId);
                var spriteData = textureMap.frames.Find(f => f.filename == frame.Sprite.FrameFilename);
                var texture = Core.Scene.Content.LoadTexture(ContentPath.Textures + Path.GetFileName(textureMap.meta.image));
                var sprite = new Sprite(
                    texture,
                    spriteData.bounds,
                    new Microsoft.Xna.Framework.Vector2(
                        spriteData.pivot.x * spriteData.sourceSize.w - spriteData.spriteSourceSize.x,
                        spriteData.pivot.y * spriteData.sourceSize.h - spriteData.spriteSourceSize.y));
                frames.Add(new ObserverFrame
                {
                    Sprite = sprite,
                    Flip = animationData.Flip,
                });
            }
            return new Animation<ObserverFrame>(frames.ToArray(), animation.FramesPerSecond);
        }
    }
}
