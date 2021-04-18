using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace Game.Entities.Factories
{
    [EntityDef("korok")]
    class Korok : EntityFactory
    {
        public override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("korok", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("prototype", Core.Scene.Content));
            anim.Play("korok_appear", Nez.Sprites.SpriteAnimator.LoopMode.ClampForever);
            anim.RenderLayer = -5;

            entity.AddComponent(new Trigger(
                self => !self.GetComponent<SpriteAnimator>().IsRunning,
                self =>
                {
                    self.GetComponent<SpriteAnimator>().Change("korok_jump");
                    self.RemoveComponent();
                }));

            return entity;
        }
    }
}
