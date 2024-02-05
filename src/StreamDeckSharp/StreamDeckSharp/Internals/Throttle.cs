using System;
using System.Diagnostics;
using System.Threading;

namespace StreamDeckSharp.Internals
{
    internal class Throttle
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private long _sumBytesInWindow = 0;
        private int _sleepCount = 0;

        public double BytesPerSecondLimit { get; set; } = double.PositiveInfinity;
        public int ByteCountBeforeThrottle { get; set; } = 16_000;

        public void MeasureAndBlock(int bytes)
        {
            _sumBytesInWindow += bytes;

            var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            var estimatedSeconds = _sumBytesInWindow / BytesPerSecondLimit;

            if (_sumBytesInWindow > ByteCountBeforeThrottle && elapsedSeconds < estimatedSeconds)
            {
                var delta = Math.Max(1, (int)((estimatedSeconds - elapsedSeconds) * 1000));
                Thread.Sleep(delta);
                _sleepCount++;
            }

            if (elapsedSeconds >= 1)
            {
                if (_sleepCount > 1)
                {
                    Debug.WriteLine($"[Throttle] {_sumBytesInWindow / elapsedSeconds}");
                }

                _stopwatch.Restart();
                _sumBytesInWindow = 0;
                _sleepCount = 0;
            }
        }
    }
}
