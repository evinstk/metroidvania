using Game.Components;
using Game.Scripts;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game.Entities.Factories
{
    [EntityDef("korok_flower")]
    class KorokFlower : EntityFactory
    {
        static Dictionary<string, int> _fps = new Dictionary<string, int>
        {
            { "korok_flower_idle", 4 },
            { "korok_flower_final_idle", 4 },
        };

        protected override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var nodes = new List<Vector2>();
            foreach (var node in ogmoEntity.nodes)
                nodes.Add(node);
            return Make(position, nodes);
        }

        Entity Make(Vector2 position, List<Vector2> nodes)
        {
            var entity = Core.Scene.CreateEntity("korok_flower", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content, _fps));
            anim.Play(nodes.Count > 0 ? "korok_flower_idle" : "korok_flower_final_idle");
            anim.RenderLayer = -5;

            var collider = entity.AddComponent<BoxCollider>();
            collider.PhysicsLayer = 0;
            collider.CollidesWithLayers = Mask.Player;

            entity.AddComponent(new Trigger(
                self => self.GetComponent<BoxCollider>().CollidesWithAny(out _),
                self =>
                {
                    if (nodes.Count > 0)
                    {
                        var next = nodes[0];
                        nodes.RemoveAt(0);
                        Make(next, nodes);
                        self.Entity.Destroy();
                    }
                    else
                    {
                        Core.Scene.GetSceneComponent<ScriptLoader>().RaiseEvent("found_korok_seed", self.Entity);
                        self.Entity.Destroy();
                    }
                }));

            return entity;
        }
    }
}
