using Engine;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Particles;

namespace Game.Entities.Factories
{
    [EntityDef("korok_trail")]
    class KorokTrail : EntityFactory
    {
        protected override Entity Create(OgmoEntity ogmoEntity, Vector2 position)
        {
            var entity = Core.Scene.CreateEntity("korok_trail", position);

            var sparks = Core.Scene.Content.LoadParticleEmitterConfig("Content/Particles/sparks.pex");
            var emitter = entity.AddComponent(new ParticleEmitter(sparks));
            emitter.RenderLayer = -15;
            emitter.OnAllParticlesExpired += self => self.Entity.Destroy();
            emitter.CollisionConfig.CollidesWithLayers = Mask.Terrain;
            emitter.CollisionConfig.Enabled = true;

            entity.AddComponent<PlatformerMover>();

            return entity;
        }
    }
}
