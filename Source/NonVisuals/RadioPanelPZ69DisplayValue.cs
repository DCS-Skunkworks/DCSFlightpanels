using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class RadioPanelPZ69DisplayValue
    {
        private RadioPanelPZ69Display _radioPanelPZ69Display;
        private RadioPanelPZ69KnobsEmulator _radioPanelPZ69Knob;
        private string _value;
        private const string SeparatorChars = "\\o/";

        public RadioPanelPZ69Display RadioPanelDisplay
        {
            get => _radioPanelPZ69Display;
            set => _radioPanelPZ69Display = value;
        }

        public string Value
        {
            get => _value;
            set => _value = value;
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Knob
        {
            get => _radioPanelPZ69Knob;
            set => _radioPanelPZ69Knob = value;
        }

        public void ImportSettings(string settings)
        {
            if (!settings.StartsWith("PZ69DisplayValue{"))
            {
                throw new Exception("Invalid setting for RadioPanelPZ69DisplayValue, cannot import.");
            }
            //PZ69DisplayValue{UpperCom1|UpperActive|124.12}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
            var tmp = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("PZ69DisplayValue{", "");
            tmp = tmp.Replace("}", "");
            //UpperCom1|UpperActive|124.12
            var array = tmp.Split('|');
            try
            {
                _value = array[2];
                _radioPanelPZ69Display = (RadioPanelPZ69Display)Enum.Parse(typeof(RadioPanelPZ69Display), array[1]);
                _radioPanelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), array[0]);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to import setting for RadioPanelPZ69Display \n" + e.Message + "\n " + e.StackTrace);
            }
        }

        public string ExportSettings()
        {
            if (!string.IsNullOrEmpty(_value) && double.Parse(_value, Common.GetPZ69FullDisplayNumberFormat()) >= 0)
            {
                return "PZ69DisplayValue{" + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), _radioPanelPZ69Knob) + "|" + Enum.GetName(typeof(RadioPanelPZ69Display), _radioPanelPZ69Display) + "|" + _value + "}";
            }
            return null;
        }
    }
}
