using Microsoft.Xna.Framework;
using Nez;
using System.IO;

namespace Game.Editor.Prefab
{
    class TiledParallaxData : DataComponent
    {
        public SpriteTextureData TextureData = new SpriteTextureData();
        public Vector2 ScrollScale = new Vector2(1, 0);
        public int RenderLayer = 99;

        public override void AddToEntity(Entity entity)
        {
            var backgroundEntity = entity.Scene.CreateEntity(Path.GetFileNameWithoutExtension(TextureData.TextureFile));
            var renderer = backgroundEntity.AddComponent(new TiledSpriteRenderer(TextureData.Texture));
            renderer.RenderLayer = RenderLayer;
            var parallax = backgroundEntity.AddComponent<TiledParallaxComponent>();
            parallax.ScrollScale = ScrollScale;
            backgroundEntity.Parent = entity.Transform;
            backgroundEntity.UpdateOrder = int.MaxValue;
            entity.Parent = entity.Scene.Camera.Transform;
        }
    }
}
