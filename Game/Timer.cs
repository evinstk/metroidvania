using Nez;

namespace Game
{
    static class Timer
    {
        public static float PauseTimer = 0;

        public static void PauseFor(float time)
        {
            if (time >= PauseTimer)
                PauseTimer = time;
        }

        public static bool OnTime(float time, float timestamp)
        {
            return time >= timestamp && time - Time.DeltaTime < timestamp;
        }

        public static bool OnInterval(float interval, float offset = 0)
        {
            return OnInterval(Time.TimeSinceSceneLoad, Time.DeltaTime, interval, offset);
        }

        public static bool OnInterval(float delta, float interval, float offset)
        {
            return OnInterval(Time.TimeSinceSceneLoad, delta, interval, offset);
        }

        public static bool OnInterval(float time, float delta, float interval, float offset)
        {
            return Mathf.Floor((time - offset - delta) / interval) < Mathf.Floor((time - offset) / interval);
        }
    }
}
