using Nez;
using Microsoft.Xna.Framework;
using Game.Editor.Prefab;
using System;

namespace Game.Editor.Tool
{
    partial class ToolWindow
    {
        class AreaTool
        {
            ToolWindow _window;
            Vector2 _start;
            bool _drawing;

            public AreaTool(ToolWindow window)
            {
                _window = window;
            }

            public void Update()
            {
                var roomData = EditorState.RoomData;

                var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                if (roomData != null
                    && Core.Scene.Camera.Bounds.Contains(worldPoint))
                {
                    worldPoint.X = Mathf.RoundToNearest(worldPoint.X, roomData.TileWidth);
                    worldPoint.Y = Mathf.RoundToNearest(worldPoint.Y, roomData.TileWidth);
                    if (Input.LeftMouseButtonPressed)
                    {
                        _start = worldPoint;
                        _drawing = true;
                    }
                    if (_drawing && Input.LeftMouseButtonReleased)
                    {
                        _drawing = false;
                        // need some area > 0
                        if (worldPoint.X != _start.X && worldPoint.Y != _start.Y)
                        {
                            var entity = new RoomEntity();
                            entity.Name = "New Area";
                            var diff = _start - worldPoint;
                            entity.Components.Add(new AreaData(
                                new Point(
                                    (int)Math.Abs(diff.X),
                                    (int)Math.Abs(diff.Y))));
                            entity.Position = new Vector2(
                                Math.Min(_start.X, worldPoint.X),
                                Math.Min(_start.Y, worldPoint.Y));
                            roomData.Entities.Add(entity);
                            EditorState.SelectedEntityId = entity.Id;
                        }
                    }
                }
            }

            public void Render(Batcher batcher, Camera camera)
            {
                var roomData = EditorState.RoomData;
                if (_drawing && roomData != null)
                {
                    var worldPoint = Core.Scene.Camera.MouseToWorldPoint();
                    worldPoint.X = Mathf.RoundToNearest(worldPoint.X, roomData.TileWidth);
                    worldPoint.Y = Mathf.RoundToNearest(worldPoint.Y, roomData.TileWidth);

                    var location = new Vector2(
                        Math.Min(_start.X, worldPoint.X),
                        Math.Min(_start.Y, worldPoint.Y));
                    var diff = _start - worldPoint;

                    batcher.DrawHollowRect(location, Math.Abs(diff.X), Math.Abs(diff.Y), Color.Pink);
                }
            }
        }
    }
}
