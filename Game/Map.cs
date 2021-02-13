using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    class OgmoProject
    {
        public List<OgmoTileset> tilesets;
    }

    class OgmoTileset
    {
        public string label;
        public string path;
        public int tileWidth;
        public int tileHeight;
    }

    class OgmoLevel
    {
        public int width;
        public int height;
        public List<OgmoLevelLayer> layers;
    }

    class OgmoLevelLayer
    {
        public int gridCellWidth;
        public int gridCellHeight;
        public int gridCellsX;
        public int gridCellsY;
        public List<int> data = new List<int>();
    }

    class MapCollider :
#if DEBUG
        RenderableComponent
#else
        Component
#endif
    {
        OgmoLevelLayer _layer;
        Collider[] _colliders;

        public MapCollider(OgmoLevelLayer layer, int width, int height)
        {
            _layer = layer;
            _width = width;
            _height = height;
        }

        public override void OnAddedToEntity()
        {
            List<Rectangle> rects = new List<Rectangle>();
            for (var i = 0; i < _layer.data.Count; ++i)
            {
                var tile = _layer.data[i];
                if (tile != -1)
                {
                    var x = i % _layer.gridCellsX * _layer.gridCellWidth;
                    var y = i / _layer.gridCellsX * _layer.gridCellHeight;
                    rects.Add(new Rectangle(x, y, _layer.gridCellWidth, _layer.gridCellHeight));
                }
            }

            _colliders = new Collider[rects.Count];
            for (var i = 0; i < rects.Count; ++i)
            {
                var collider = new BoxCollider(rects[i]);
                collider.PhysicsLayer = Mask.Terrain;
                collider.Entity = Entity;
                _colliders[i] = collider;
                Physics.AddCollider(collider);
            }
        }

#if DEBUG
        public override float Width => _width;
        float _width = 1;
        public override float Height => _height;
        float _height = 1;

        public override void Render(Batcher batcher, Camera camera)
        {
        }

        public override void DebugRender(Batcher batcher)
        {
            foreach (var collider in _colliders)
                collider.DebugRender(batcher);
        }
#endif
    }
}
