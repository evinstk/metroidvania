using Game.Scripts;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game.Entities.Factories
{
    [EntityDef("korok_platform")]
    class KorokPlatform : EntityFactory
    {
        Dictionary<string, int> _fps = new Dictionary<string, int>
        {
            { "korok_platform_active", 15 },
        };

        public override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("korok_platform", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content, _fps));
            anim.Play("korok_platform_active");
            anim.RenderLayer = -15;

            var collider = entity.AddComponent(new BoxCollider(-14, 12, 28, 4));
            collider.PhysicsLayer = Mask.Terrain;

            var triggerArea = entity.AddComponent(new BoxCollider(-1, 11, 2, 1));
            triggerArea.PhysicsLayer = 0;
            triggerArea.CollidesWithLayers = Mask.Player;

            entity.AddComponent(new Trigger(
                self => triggerArea.CollidesWithAny(out _),
                self => Core.Scene.GetSceneComponent<ScriptLoader>().RaiseEvent("korok_platform_hit", self.Entity),
                true));

            return entity;
        }
    }
}
