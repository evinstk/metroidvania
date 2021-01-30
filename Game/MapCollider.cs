using Game.Editor;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game
{
    class MapCollider : RenderableComponent
    {
        public int PhysicsLayer = 1 << 0;
        public bool IsTrigger = false;
        public override float Width => _width * _tileWidth;
        public override float Height => _height * _tileHeight;

        Collider[] _colliders;
        int _width;
        int _height;
        int _tileWidth;
        int _tileHeight;
        bool[] _hasTile;

        public MapCollider(RoomData roomData, int index)
        {
            _width = roomData.RoomWidth;
            _height = roomData.RoomHeight;
            _tileWidth = roomData.TileWidth;
            _tileHeight = roomData.TileHeight;
            _hasTile = new bool[_width * _height];
            var position = roomData.Position;

            var layer = roomData.Layers[index];
            foreach (var tile in layer.Tiles)
            {
                var loc = tile.LayerLocation;
                _hasTile[loc.X - position.X + (loc.Y - position.Y) * _width] = true;
            }
        }

        public override void OnAddedToEntity()
        {
            AddColliders();
        }

        public override void OnRemovedFromEntity()
        {
            RemoveColliders();
        }

        public override void OnEntityTransformChanged(Transform.Component comp)
        {
            if (comp == Transform.Component.Position)
            {
                RemoveColliders();
                AddColliders();
            }
        }

        void AddColliders()
        {
            var rects = GetCollisionRectangles();
            _colliders = new Collider[rects.Count];
            for (var i = 0; i < rects.Count; ++i)
            {
                var collider = new BoxCollider(
                    rects[i].X,
                    rects[i].Y,
                    rects[i].Width,
                    rects[i].Height);
                collider.PhysicsLayer = PhysicsLayer;
                collider.Entity = Entity;
                collider.IsTrigger = IsTrigger;
                _colliders[i] = collider;
                Physics.AddCollider(collider);
            }
        }

        List<Rectangle> GetCollisionRectangles()
        {
            var rectangles = new List<Rectangle>();
            var checkedIndexes = new bool?[_hasTile.Length];
            var startCol = -1;
            //var index = -1;

            for (var y = 0; y < _height; ++y)
            {
                for (var x = 0; x < _width; ++x)
                {
                    var index = y * _width + x;
                    var hasTile = _hasTile[index];

                    if (hasTile && (checkedIndexes[index] == false || checkedIndexes[index] == null))
                    {
                        if (startCol < 0)
                            startCol = x;
                        checkedIndexes[index] = true;
                    }
                    else if (!hasTile || checkedIndexes[index] == true)
                    {
                        if (startCol >= 0)
                        {
                            rectangles.Add(FindBoundsRect(startCol, x, y, checkedIndexes));
                            startCol = -1;
                        }
                    }
                } // end x pass

                if (startCol >= 0)
                {
                    rectangles.Add(FindBoundsRect(startCol, _width, y, checkedIndexes));
                    startCol = -1;
                }
            }

            return rectangles;
        }

        Rectangle FindBoundsRect(int startX, int endX, int startY, bool?[] checkedIndexes)
        {
            for (var y = startY + 1; y < _height; ++y)
            {
                for (var x = startX; x < endX; ++x)
                {
                    var index = y * _width + x;
                    var hasTile = _hasTile[index];
                    if (!hasTile || checkedIndexes[index] == true)
                    {
                        // clear row since it's incomplete for rectangle
                        for (var _x = startX; _x < x; ++_x)
                        {
                            checkedIndexes[y * _width + _x] = false;
                        }

                        return new Rectangle(
                            startX * _tileWidth,
                            startY * _tileHeight,
                            (endX - startX) * _tileWidth,
                            (y - startY) * _tileHeight);
                    }
                    checkedIndexes[index] = true;
                }
            }

            return new Rectangle(
                startX * _tileWidth,
                startY * _tileHeight,
                (endX - startX) * _tileWidth,
                (_height - startY) * _tileHeight);
        }

        void RemoveColliders()
        {
            if (_colliders != null)
            {
                foreach (var collider in _colliders)
                    Physics.RemoveCollider(collider);
                _colliders = null;
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
        }

        public override void DebugRender(Batcher batcher)
        {
            foreach (var collider in _colliders)
                collider.DebugRender(batcher);
        }
    }
}
