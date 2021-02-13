using Nez;

namespace Game
{
    static class Timer
    {
        public static bool OnTime(float time, float timestamp)
        {
            return time >= timestamp && time - Time.DeltaTime < timestamp;
        }
    }
}
