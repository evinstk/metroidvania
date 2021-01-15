using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ImGuiTools.TypeInspectors;
using System.IO;

namespace Game.Editor.Prefab
{
    class TiledParallaxData : DataComponent
    {
        public TextureData TextureData = new TextureData();
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

    [CustomInspector(typeof(SpriteTextureDataInspector))]
    class TextureData
    {
        public string TextureFile;

        public Texture2D Texture => Core.Scene.Content.LoadTexture(ContentPath.Textures + TextureFile);
    }

    class SpriteTextureDataInspector : AbstractTypeInspector
    {
        string[] _textureFiles;

        public SpriteTextureDataInspector()
        {
            _textureFiles = Directory.GetFiles(ContentPath.Textures, "*.png");
        }

        public override void DrawMutable()
        {
            var texData = GetValue<TextureData>();
            if (ImGui.BeginCombo("Texture", texData.TextureFile))
            {
                foreach (var file in _textureFiles)
                {
                    var name = Path.GetFileName(file);
                    if (ImGui.Selectable(name, name == texData.TextureFile))
                    {
                        texData.TextureFile = name;
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
}
