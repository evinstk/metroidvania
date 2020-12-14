using Microsoft.Xna.Framework;
using Nez;

namespace Game
{
    class SimpleMover : Component, IUpdatable
    {
        public int Speed = 100;

        ControllerComponent _controller;
        SubpixelVector2 _subpixelV2 = new SubpixelVector2();
        Mover _mover;

        public override void OnAddedToEntity()
        {
            _controller = Entity.GetComponent<ControllerComponent>();
            Insist.IsNotNull(_controller);

            _mover = Entity.AddComponent<Mover>();
        }

        public void Update()
        {
            var movement = new Vector2(_controller.XAxis, _controller.YAxis) * Speed * Time.DeltaTime;
            if (movement != Vector2.Zero)
            {
                _mover.CalculateMovement(ref movement, out _);
                _subpixelV2.Update(ref movement);
                _mover.ApplyMovement(movement);
            }
        }

        public SimpleMover SetSpeed(int speed)
        {
            Speed = speed;
            return this;
        }
    }
}
