using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Persistence;
using System;

namespace Game.Editor.Prefab
{
    [Serializable]
    class SpriteData : PrefabComponent
    {
        public string TextureFile;
        public Rectangle SourceRect;
        public Vector2 Origin;
        [JsonExclude]
        public Color Color = Color.White;

        public Texture2D Texture => Core.Scene.Content.LoadTexture("Content/Textures/" + TextureFile);
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
}
