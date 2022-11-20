namespace NonVisuals
{
    using System;
    using System.Collections.Generic;

    public class ClickSpeedDetector
    {
        private readonly List<long> _clicksTimeTicksList = new();
        private readonly int _maxClicksToKeep  = 200;
        private readonly int _defaultPeriod  = 2000;
        private readonly int _clickCountThreshold;

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
