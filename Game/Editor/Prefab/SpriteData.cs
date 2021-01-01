using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ImGuiTools.TypeInspectors;
using Nez.Persistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Editor.Prefab
{
    [Serializable]
    class SpriteData : PrefabComponent
    {
        public SpriteTextureData TextureData = new SpriteTextureData();
        public Rectangle SourceRect;
        public Vector2 Origin;
        [JsonExclude]
        public Color Color = Color.White;
    }

    class SpriteDataConverter : JsonTypeConverter<SpriteData>
    {
        public override void OnFoundCustomData(SpriteData instance, string key, object value)
        {
            if (key == "Color")
            {
                instance.Color = new Color(Convert.ToUInt32(value));
            }
        }

        public override void WriteJson(IJsonEncoder encoder, SpriteData value)
        {
            encoder.EncodeKeyValuePair("Color", value.Color.PackedValue);
        }
    }

    [CustomInspector(typeof(SpriteTextureDataInspector))]
    class SpriteTextureData
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
            var texData = GetValue<SpriteTextureData>();
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
