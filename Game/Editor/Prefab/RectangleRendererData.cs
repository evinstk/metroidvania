using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System;

namespace Game.Editor.Prefab
{
    [Serializable]
    class RectangleRendererData : DataComponent
    {
        public Vector2 Size = new Vector2(32, 32);
        [JsonExclude]
        public Color Color = Color.White;

        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent(new RectangleRenderer(Color, Size.X, Size.Y));
        }

        public override void Render(Batcher batcher, Vector2 position)
        {
            var rect = new RectangleF(
                position - Size / 2,
                Size);
            batcher.DrawRect(
                rect,
                Color);
        }

        public override bool Select(Vector2 entityPosition, Vector2 mousePosition)
        {
            var rect = new RectangleF(
                entityPosition - Size / 2,
                Size);
            return rect.Contains(mousePosition);
        }
    }

    class RectangleRendererDataConverter : JsonTypeConverter<RectangleRendererData>
    {
        public override void OnFoundCustomData(RectangleRendererData instance, string key, object value)
        {
            if (key == "Color")
            {
                instance.Color = new Color(Convert.ToUInt32(value));
            }
        }

        public override void WriteJson(IJsonEncoder encoder, RectangleRendererData value)
        {
            encoder.EncodeKeyValuePair("Color", value.Color.PackedValue);
        }
    }
}
