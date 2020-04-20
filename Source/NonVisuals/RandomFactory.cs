using System;

namespace NonVisuals
{
    public static class RandomFactory
    {
        private static Random _random = new Random(DateTime.Now.Millisecond);

        public static int Get()
        {
            return _random.Next(Int32.MaxValue);
        }
    }
}
