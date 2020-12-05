using Game.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.IO;
using System.Linq;

namespace Game
{
    class FrameOptions
    {
        public bool FlipX = false;
        public TiledObject HitBoxData;
        public SoundEffect Sound;
    }

    class Frame : IFrame
    {
        SpriteRenderer _renderer;
        HitBoxComponent _hitC;

        Sprite _sprite;
        FrameOptions _options;

        public Frame(
            SpriteRenderer renderer,
            HitBoxComponent hitC,
            Sprite sprite,
            FrameOptions options)
        {
            _renderer = renderer;
            _hitC = hitC;
            _sprite = sprite;
            _options = options;
        }

        public void OnEnter()
        {
            _renderer.Sprite = _sprite;
            _renderer.FlipX = _options.FlipX;

            var hitBoxData = _options.HitBoxData;
            _hitC?.SetEnabled(hitBoxData != null);
            if (hitBoxData != null)
            {
                _hitC.Collider.SetSize(hitBoxData.Width, hitBoxData.Height);
                _hitC.Collider.SetLocalOffset(new Vector2(
                    (hitBoxData.X + .5f * (hitBoxData.Width - _renderer.Bounds.Width)) * (_options.FlipX ? -1 : 1),
                    hitBoxData.Y + .5f * (hitBoxData.Height - _renderer.Bounds.Height)));
            }

            var sound = _options.Sound;
            if (sound != null)
            {
                sound.Play();
            }
        }
    }

    static class EntityExt
    {
        public static Animator<Frame> AddAnimator(this Entity entity, string animationGroup)
        {
            var animator = new Animator<Frame>();

            var group = Core.Content.Load<Data.AnimationGroup[]>("Data/AnimationGroups").First(g => g.Name == animationGroup);

            var spriteRenderer = entity.GetComponent<SpriteRenderer>();
            Insist.IsNotNull(spriteRenderer);
            var hitC = entity.GetComponent<HitBoxComponent>();
            Insist.IsNotNull(hitC);

            foreach (var animType in group.AnimationTypes)
            {
                var tileset = Tileset.Load("Content/Animations/" + animType.Tileset + ".json");
                var anim = tileset.Tiles.First(t => t.Type == animType.Name);
                var texture = entity.Scene.Content.LoadTexture("Textures/" + Path.GetFileNameWithoutExtension(tileset.Image));
                var sprites = Sprite.SpritesFromAtlas(texture, tileset.TileWidth, tileset.TileHeight);
                var tileMeta = tileset.Tiles
                    .ToDictionary(t => t.Id);
                var frames = anim.Animation.Select(f =>
                {
                    var sprite = sprites[f.TileId];
                    var opts = new FrameOptions();
                    opts.FlipX = animType.Flip;
                    if (tileMeta.TryGetValue(f.TileId, out var meta))
                    {
                        foreach (var obj in meta.ObjectGroup.Objects)
                        {
                            if (obj.Type == "hit")
                            {
                                opts.HitBoxData = obj;
                            }
                        }
                        foreach (var prop in meta.Properties)
                        {
                            if (prop.Name == "sound" && prop.Type == "file")
                            {
                                opts.Sound = entity.Scene.Content.Load<SoundEffect>("Sounds/" + Path.GetFileNameWithoutExtension(prop.Value));
                            }
                        }
                    }
                    return new Frame(
                        spriteRenderer,
                        hitC,
                        sprite,
                        opts);
                }).ToArray();
                animator.AddAnimation(
                    animType.Type,
                    new Animation<Frame>(
                        frames,
                        12));
            }

            return entity.AddComponent(animator);
        }
    }
}
