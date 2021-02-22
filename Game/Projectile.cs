using Nez;

namespace Game
{
    class Projectile : Component, IUpdatable
    {
        //public Vector2 Direction = new Vector2(1, 0);
        public float TimeToLive = 2f;

        float _timer = 0;

        public void Update()
        {
            _timer += Time.DeltaTime;

            if (_timer > TimeToLive)
            {
                Entity.Destroy();
            }
        }
    }
}
