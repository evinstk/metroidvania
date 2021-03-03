using System;

namespace Game
{
    static class Calc
    {
        static Random rnd = new Random();

        public static int RandInt(int maxExc)
        {
            if (maxExc <= 0)
                return 0;
            return rnd.Next(0, maxExc);
        }

        public static int RandInt(int min, int maxExc)
        {
            return rnd.Next(min, maxExc);
        }
    }
}
