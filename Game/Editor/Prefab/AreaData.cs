using Microsoft.Xna.Framework;
using Nez;

namespace Game.Editor.Prefab
{
    class AreaData : DataComponent
    {
        public Point Size;

        public AreaData() { }
        public AreaData(Point size)
        {
            Size = size;
        }

        public override void AddToEntity(Entity entity)
        {
            var collider = entity.AddComponent(new BoxCollider(0, 0, Size.X, Size.Y));
            collider.IsTrigger = true;
        }

        public override void Render(Batcher batcher, Vector2 position)
        {
            var color = Color.DarkMagenta;
            color.A = 1;
            batcher.DrawRect(position, Size.X, Size.Y, color);
        }

        public override bool Select(Vector2 entityPosition, Vector2 mousePosition)
        {
            var bounds = new RectangleF(entityPosition, Size.ToVector2());
            return bounds.Contains(mousePosition);
        }
    }
}
