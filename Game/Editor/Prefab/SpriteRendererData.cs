using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.IO;

namespace Game.Editor.Prefab
{
    [Serializable]
    class SpriteRendererData : DataComponent
    {
        public TextureMapSpriteData SpriteData = new TextureMapSpriteData();
        public bool Flip;
        public int RenderLayer = 0;

        public override void AddToEntity(Entity entity)
        {
            var sprite = SpriteData.Sprite;
            var renderer = entity.AddComponent(new SpriteRenderer(sprite));
            renderer.Color = SpriteData.Color;
            renderer.FlipX = Flip;
            renderer.RenderLayer = RenderLayer;
        }

        public override void Render(Batcher batcher, Vector2 position)
        {
            var sprite = SpriteData.Sprite;
            if (sprite != null)
            {
                batcher.Draw(sprite, position, SpriteData.Color, 0f, sprite.Origin, 1f, Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
            else
            {
                batcher.DrawPixel(position, Color.Green, 3);
            }
        }

        public override bool Select(Vector2 entityPosition, Vector2 mousePosition)
        {
            var frame = SpriteData.Frame;
            var rect = new Rectangle(
                (entityPosition - frame.Origin).ToPoint(),
                new Point(frame.bounds.w, frame.bounds.h));
            return rect.Contains(mousePosition);
        }
    }

    [CustomInspector(typeof(TextureMapSpriteDataInspector))]
    class TextureMapSpriteData
    {
        public string TextureMapId = "";
        public string FrameFilename = "";
        [JsonExclude]
        public Color Color = Color.White;

        public Animation.TextureMapData TextureMap => Core.GetGlobalManager<Animation.TextureMapDataManager>().GetResource(TextureMapId);
        public Animation.Frame Frame => TextureMap.frames.Find(f => f.filename == FrameFilename);

        public Sprite Sprite
        {
            get
            {
                if (string.IsNullOrEmpty(TextureMapId) || string.IsNullOrEmpty(FrameFilename)) return null;
                var textureMap = Core.GetGlobalManager<Animation.TextureMapDataManager>().GetResource(TextureMapId);
                var texture = Core.Scene.Content.LoadTexture(ContentPath.Textures + Path.GetFileName(textureMap.meta.image));
                var frame = textureMap.frames.Find(f => f.filename == FrameFilename);
                var sprite = new Sprite(
                    texture,
                    frame.bounds,
                    frame.Origin);
                return sprite;
            }
        }

    }

    class TextureMapSpriteDataInspector : AbstractTypeInspector
    {
        bool _isHeaderOpen;
        AbstractTypeInspector _colorInspector;

        public override void Initialize()
        {
            base.Initialize();

            var obj = GetValue();

            var colorField = _valueType.GetField(nameof(TextureMapSpriteData.Color));
            _colorInspector = TypeInspectorUtils.GetInspectorForType(colorField.FieldType, obj, colorField);
            _colorInspector.SetTarget(obj, _valueType.GetField(nameof(TextureMapSpriteData.Color)));
            _colorInspector.Initialize();
        }

        public override void DrawMutable()
        {
            ImGui.Indent();
            NezImGui.BeginBorderedGroup();

            _isHeaderOpen = ImGui.CollapsingHeader($"{_name}");
            if (_isHeaderOpen)
            {
                var obj = GetValue<TextureMapSpriteData>();
                var manager = Core.GetGlobalManager<Animation.TextureMapDataManager>();
                if (manager.Combo("Texture Map", ref obj.TextureMapId))
                    obj.FrameFilename = null;
                manager.FrameCombo(obj.TextureMapId, ref obj.FrameFilename);

                _colorInspector.Draw();
            }

            NezImGui.EndBorderedGroup(new System.Numerics.Vector2(4, 1), new System.Numerics.Vector2(4, 2));
            ImGui.Unindent();
        }
    }

    class TextureMapSpriteDataConverter : JsonTypeConverter<TextureMapSpriteData>
    {
        public override void OnFoundCustomData(TextureMapSpriteData instance, string key, object value)
        {
            if (key == "Color")
            {
                instance.Color = new Color(Convert.ToUInt32(value));
            }
        }

        public override void WriteJson(IJsonEncoder encoder, TextureMapSpriteData value)
        {
            encoder.EncodeKeyValuePair("Color", value.Color.PackedValue);
        }
    }
}
