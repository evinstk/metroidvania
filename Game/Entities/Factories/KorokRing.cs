using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System.Collections.Generic;

namespace Game.Entities.Factories
{
    [EntityDef("korok_ring")]
    class KorokRing : EntityFactory
    {
        static Dictionary<string, int> _fps = new Dictionary<string, int>()
        {
            { "korok_orb_start", 16 },
            { "korok_orb_idle", 16 },
        };
        static float Radius = 32f;
        static int OrbCount = 8;

        protected override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("korok_ring", position);

            for (var i = 0; i < OrbCount; ++i)
            {
                var orb = Orb();
                orb.Parent = entity.Transform;
                orb.LocalPosition = Mathf.AngleToVector(Mathf.Deg2Rad * (i / (float)OrbCount) * 360, Radius);
            }

            return entity;
        }

        Entity Orb()
        {
            var entity = Core.Scene.CreateEntity("orb");

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content, _fps));
            anim.Play("korok_orb_start", Nez.Sprites.SpriteAnimator.LoopMode.ClampForever);
            anim.RenderLayer = -15;

            entity.AddComponent(new Trigger(
                self => !self.GetComponent<SpriteAnimator>().IsRunning,
                self => self.GetComponent<SpriteAnimator>().Play("korok_orb_idle")));

            return entity;
        }
    }
}
