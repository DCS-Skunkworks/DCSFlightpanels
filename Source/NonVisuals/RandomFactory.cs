namespace NonVisuals
{
    using System;

    public static class RandomFactory
    {
        private static Random _random = new Random(DateTime.Now.Millisecond);

        public static int Get()
        {
            return _random.Next(int.MaxValue);
        }
    }
}
