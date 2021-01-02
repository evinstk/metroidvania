using Game.Editor;
using Game.MobState;
using Game.Movement;
using Nez;
using Nez.Sprites;

namespace Game.Prefabs
{
    static class Hero
    {
        [Prefab("Hero")]
        public static Entity MakeHero(string name)
        {
            var entity = new Entity(name);

            var textureMap = IO.Json.ReadJson<Data.TextureMap.TextureMap>("Content/Textures/hero32.json");
            //var texture = Core.Scene.Content.Load<Texture2D>(
            //    "Textures/" + Path.GetFileNameWithoutExtension(textureMap.Meta.Image));
            //var groups = textureMap.GetGroups();
            //var idleAnimation = new SpriteAnimation(groups["heroIdle"].Select(f => new Sprite(texture, f.Bounds)).ToArray(), 12);
            //var frame = groups["heroIdle"].First();
            //var sprite = new Sprite(texture, frame.Bounds);

            //entity.AddComponent(new SpriteRenderer(sprite));
            //var animator = entity.AddComponent(new SpriteAnimator(idleAnimation.Sprites[0]));

            //entity
            //    .AddComponent<BoxCollider>()
            //    .AddComponent<CollisionComponent>()
            //    .AddComponent<PlayerController>();

            entity.AddComponent<SpriteRenderer>().SetRenderLayer(-1);
            var animator = entity.AddAnimator(textureMap);
            //animator.Play("default");

            entity.AddComponent(new BoxCollider(12, 32));
            entity.AddComponent<PlayerController>();
            entity.AddComponent<PlayerMovement>();
            var followCamera = entity.AddComponent(new FollowCamera(entity, FollowCamera.CameraStyle.CameraWindow));
            followCamera.FollowLerp = 1;

            //entity.AddComponent<MobStateMachine>();

            return entity;
        }
    }
}
