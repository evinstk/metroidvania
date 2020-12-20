using Game.Editor;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.IO;
using System.Linq;

namespace Game.Prefabs
{
    static class Hero
    {
        [Prefab("Hero")]
        public static Entity MakeHero(string name)
        {
            var entity = new Entity(name);

            var textureMap = Json.ReadJson<Data.TextureMap.TextureMap>("Content/Textures/hero32.json");
            //var texture = Core.Scene.Content.Load<Texture2D>(
            //    "Textures/" + Path.GetFileNameWithoutExtension(textureMap.Meta.Image));
            //var groups = textureMap.GetGroups();
            //var idleAnimation = new SpriteAnimation(groups["heroIdle"].Select(f => new Sprite(texture, f.Bounds)).ToArray(), 12);
            //var frame = groups["heroIdle"].First();
            //var sprite = new Sprite(texture, frame.Bounds);

            //entity.AddComponent(new SpriteRenderer(sprite));
            //var animator = entity.AddComponent(new SpriteAnimator(idleAnimation.Sprites[0]));

            entity.AddComponent<SpriteRenderer>();
            var animator = entity.AddAnimator(textureMap);
            animator.Play("heroIdle");

            return entity;
        }
    }
}
