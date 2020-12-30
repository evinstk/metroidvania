using Microsoft.Xna.Framework;
using Nez.Persistence;
using System;

namespace Game.Editor.Prefab
{
    [Serializable]
    class LightData : PrefabComponent
    {
        public float Radius = 200f;
        public float Power = 1f;
        public Vector2 LocalOffset = Vector2.Zero;
        [JsonExclude]
        public Color Color = Color.White;
    }

    class LightDataConverter : JsonTypeConverter<LightData>
    {
        public override void OnFoundCustomData(LightData instance, string key, object value)
        {
            if (key == "Color")
            {
                instance.Color = new Color(Convert.ToUInt32(value));
            }
        }

        public override void WriteJson(IJsonEncoder encoder, LightData value)
        {
            encoder.EncodeKeyValuePair("Color", value.Color.PackedValue);
        }
    }
}
