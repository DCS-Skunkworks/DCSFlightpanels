using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public class ClickSpeedDetector
    {
        private readonly List<long> _clicksTimeTicksList = new List<long>();
        private int _maxClicksToKeep = 200;
        private int _defaultPeriod = 2000;
        private int _clickCountThreshold = 20;

        public ClickSpeedDetector()
        {
        }

        public ClickSpeedDetector(int upperClickThreshold)
        {
            _clickCountThreshold = upperClickThreshold;
        }

        public ClickSpeedDetector(int maxClicksToKeep, int defaultPeriod, int clickCountThreshold)
        {
            _maxClicksToKeep = maxClicksToKeep;
            _defaultPeriod = defaultPeriod;
            _clickCountThreshold = clickCountThreshold;
        }

        public int ClicksWithinLastPeriod(int millisecondPeriod)
        {
            var result = 0;
            var millisecNow = DateTime.Now.Ticks / 10000;
            foreach (var l in _clicksTimeTicksList)
            {
                if (l / 10000 > millisecNow - millisecondPeriod)
                {
                    //we are within period
                    result++;
                }
            }

            return result;
        }

        public int ClicksWithinDefaultPeriod()
        {
            var result = 0;
            var millisecNow = DateTime.Now.Ticks / 10000;
            foreach (var l in _clicksTimeTicksList)
            {
                if (l / 10000 > millisecNow - _defaultPeriod)
                {
                    //we are within period
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

        public int MaxClicksToKeep
        {
            get { return _maxClicksToKeep; }
            set { _maxClicksToKeep = value; }
        }

        public int DefaultPeriod
        {
            get { return _defaultPeriod; }
            set { _defaultPeriod = value; }
        }

        public int ClickCountThreshold
        {
            get { return _clickCountThreshold; }
            set { _clickCountThreshold = value; }
        }
    }
}
