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
        };

        public override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("korok_flower", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content, _fps));
            anim.Play("korok_flower_idle");
            anim.RenderLayer = -5;

            return entity;
        }
    }
}
