using Game.Editor.Prefab;
using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace Game
{
    [EntityOnly(typeof(ElevatorEntityData))]
    class ElevatorData : DataComponent
    {
        public int Speed = 100;

        public override void AddToEntity(Entity entity)
        {
            var elevator = entity.AddComponent<Elevator>();
            elevator.Speed = Speed;
        }
    }

    class ElevatorEntityData : EntityOnlyComponent
    {
        public List<int> Stops = new List<int>();

        public override void AddToEntity(Entity entity)
        {
            var elevator = entity.GetComponent<Elevator>();
            elevator.Stops.Add(entity.Position);
            foreach (var stop in Stops)
                elevator.Stops.Add(new Vector2(0, stop) + entity.Position);
        }

        List<Vector2> _stops = new List<Vector2>();
        public override void Render(Batcher batcher, Vector2 position)
        {
            _stops.Clear();
            _stops.Add(position);
            foreach (var stop in Stops)
                _stops.Add(new Vector2(0, stop) + position);

            batcher.DrawPoints(_stops, Color.AliceBlue, 1f);
            foreach (var stop in _stops)
                batcher.DrawPixel(stop, Color.Crimson, 3);
        }
    }

    class Elevator : Component, IUpdatable
    {
        public int Speed;
        public List<Vector2> Stops = new List<Vector2>();
        public float WaitDuration = 2f;

        int _nextStopIndex;
        SubpixelFloat _remainderY;
        float _waitElapsed = 0;

        public void Update()
        {
            var nextStop = Stops[_nextStopIndex];
            if (nextStop == Entity.Position)
            {
                _nextStopIndex = (_nextStopIndex + 1) % Stops.Count;
                nextStop = Stops[_nextStopIndex];
                _waitElapsed = 0;
            }

            if (_waitElapsed < WaitDuration)
            {
                _waitElapsed += Time.DeltaTime;
                return;
            }

            var motion = nextStop - Entity.Position;
            motion.Normalize();
            motion *= Speed * Time.DeltaTime;
            _remainderY.Update(ref motion.Y);
            Entity.Position += motion;
        }
    }
}
