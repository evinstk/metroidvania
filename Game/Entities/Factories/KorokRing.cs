using Game.Components;
using Game.Scripts;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
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
            { "korok_orb_small_idle", 16 },
        };
        const float Radius = 32f;
        const int OrbCount = 12;

        protected override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("korok_ring", position);

            var duration = Convert.ToSingle(ogmoEntity.values["duration"]);
            var orbs = new List<Entity>(OrbCount);
            for (var i = 0; i < OrbCount; ++i)
            {
                var orb = Orb();
                orb.Parent = entity.Transform;
                var ratio = (i + 1) / (float)OrbCount;
                orb.LocalPosition = Mathf.AngleToVector(Mathf.Deg2Rad * (ratio * 360 - 90), Radius);
                if (i != OrbCount - 1)
                    orb.AddComponent(new TimerComponent(
                        duration * ratio,
                        self => self.GetComponent<SpriteAnimator>().Play("korok_orb_small_idle")));
                orbs.Add(orb);
            }

            entity.AddComponent(new TimerComponent(
                duration,
                self =>
                {
                    foreach (var orb in orbs)
                    {
                        orb.GetComponent<SpriteAnimator>().Play("korok_orb_startReverse", SpriteAnimator.LoopMode.ClampForever);
                    }
                    self.AddComponent(new Trigger(
                        t => !orbs[0].GetComponent<SpriteAnimator>().IsRunning,
                        t => t.Entity.Destroy()));
                }));

            var collider = entity.AddComponent(new CircleCollider(Radius));
            collider.PhysicsLayer = 0;
            collider.CollidesWithLayers = Mask.Player;

            entity.AddComponent(new Trigger(
                self => self.GetComponent<CircleCollider>().CollidesWithAny(out _),
                self => Core.Scene.GetSceneComponent<ScriptLoader>().RaiseEvent("korok_ring_enter", self.Entity),
                true));

            return entity;
        }

        Entity Orb()
        {
            var entity = Core.Scene.CreateEntity("orb");

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content, _fps));
            anim.Play("korok_orb_start", SpriteAnimator.LoopMode.ClampForever);
            anim.RenderLayer = -15;

            entity.AddComponent(new Trigger(
                self => !self.GetComponent<SpriteAnimator>().IsRunning,
                self =>
                {
                    self.GetComponent<SpriteAnimator>().Play("korok_orb_idle");
                    self.RemoveComponent();
                }));

            return entity;
        }
    }
}
