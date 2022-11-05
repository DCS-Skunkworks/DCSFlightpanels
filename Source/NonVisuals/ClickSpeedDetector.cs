namespace NonVisuals
{
    using System;
    using System.Collections.Generic;

    public class ClickSpeedDetector
    {
        private readonly List<long> _clicksTimeTicksList = new();
        private int _maxClicksToKeep { get; set; } = 200;
        private int _defaultPeriod { get; set; } = 2000;
        private int _clickCountThreshold { get; set; } = 20;

        public ClickSpeedDetector(int upperClickThreshold)
        {
            _clickCountThreshold = upperClickThreshold;
        }

        public int ClicksWithinLastPeriod(int millisecondPeriod)
        {
            var result = 0;
            var millisecNow = DateTime.Now.Ticks / 10000;
            foreach (var l in _clicksTimeTicksList)
            {
                if (l / 10000 > millisecNow - millisecondPeriod)
                {
                    // we are within period
                    result++;
                }
            }
            return result;
        }

        private int ClicksWithinDefaultPeriod()
        {
            var result = 0;
            var millisecNow = DateTime.Now.Ticks / 10000;
            foreach (var l in _clicksTimeTicksList)
            {
                if (l / 10000 > millisecNow - _defaultPeriod)
                {
                    // we are within period
                    result++;
                }
            }
            return result;
        }

        public bool ClickAndCheck()
        {
            Click();
            return ClickThresholdReached();
        }

        public bool ClickThresholdReached()
        {
            var count = ClicksWithinDefaultPeriod();
            return count >= _clickCountThreshold;
        }

        public void Click()
        {
            _clicksTimeTicksList.Add(DateTime.Now.Ticks);
            ClearList();
        }

        private void ClearList()
        {
            while (_clicksTimeTicksList.Count > _maxClicksToKeep)
            {
                _clicksTimeTicksList.RemoveAt(0);
            }
        }
    }
}
