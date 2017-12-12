using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NonVisuals
{
    abstract class PerformanceTimer
    {
        private string _id;
        private List<KeyValuePair<bool, long>> _clickTimesList;
        private long _start = DateTime.Now.Ticks;

        public PerformanceTimer(string id)
        {
            _clickTimesList = new List<KeyValuePair<bool, long>>(500000);
            _id = id;
        }

        public void ClickStart()
        {
            //There are 10,000 ticks in a millisecond
            _clickTimesList.Add(new KeyValuePair<bool, long>(true, DateTime.Now.Ticks));
        }

        public void ClickEnd()
        {
            //There are 10,000 ticks in a millisecond
            _clickTimesList.Add(new KeyValuePair<bool, long>(false, DateTime.Now.Ticks));
        }

        public void PrintConsole()
        {
            try
            {
                var list = GetResultList();
                for (var i = 0; i < list.Count; i++)
                {
                    Console.WriteLine(_clickTimesList[i]);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(11222063, ex);
            }
        }

        public void PrintToFile(string filename)
        {
            try
            {
                var list = GetResultList();

                var stringBuilder = new StringBuilder();
                for (var i = 0; i < list.Count; i++)
                {
                    stringBuilder.AppendLine(list[i]);
                }
                File.WriteAllText(filename, stringBuilder.ToString(), Encoding.ASCII);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(11222063, ex);
            }
        }

        private List<string> GetResultList()
        {
            var result = new List<string>();
            try
            {
                var header = DateTime.Now.ToString() + "\n\n" + _id + "\n------------";

                result.Add(header);
                for (var i = 0; i < _clickTimesList.Count; i++)
                {
                    var kvp = _clickTimesList[i];
                    long diff = 0;
                    if (i > 0)
                    {
                        var previousKvp = _clickTimesList[i - 1];
                        diff = kvp.Value - previousKvp.Value;
                        if (kvp.Key)
                        {
                            result.Add("Start : " + (kvp.Value - _start) / 10000 + " ms");//", diff from previous " + diff/10000 + " ms.");
                        }
                        else
                        {
                            result.Add("End : " + (kvp.Value - _start) / 10000 + " ms, diff from previous " + diff / 10000 + " ms.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(11222063, ex);
            }
            return result;
        }
    }
}
