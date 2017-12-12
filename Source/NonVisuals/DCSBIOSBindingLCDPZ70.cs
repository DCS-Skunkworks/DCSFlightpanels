﻿using System;
using DCS_BIOS;

namespace NonVisuals
{
    public enum PZ70LCDPosition
    {
        UpperLCD,
        LowerLCD,
    }

    public class DCSBIOSBindingLCDPZ70
    {
        /*
         * This class binds a LCD row on the PZ70 with a DCSBIOSOutput
         * 
         * The comparison part of the DCSBIOSOutput is ignored for DCSBIOSBindingLCDPZ70, all data will be shown
         */
        private int _currentValue = 0;
        private PZ70DialPosition _pz70DialPosition;
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula; //If this is set to !null value then ignore the _dcsbiosOutput
        private const string SeparatorChars = "\\o/";
        private PZ70LCDPosition _pz70LCDPosition;

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }
            if (settings.StartsWith("MultiPanelDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutput{"))
            {
                //MultiPanelDCSBIOSControlLCD{ALT}\o/{LowerLCD}\o/DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //MultiPanelDCSBIOSControlLCD{ALT}
                var param0 = parameters[0].Replace("MultiPanelDCSBIOSControlLCD{", "").Replace("}", "");
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                //[1]
                //{LowerLCD}
                var param1 = parameters[1].Replace("{", "").Replace("}", "").Trim();
                _pz70LCDPosition = (PZ70LCDPosition)Enum.Parse(typeof(PZ70LCDPosition), param1);

                //[2]
                //DCSBiosOutput{ANT_EGIHQTOD|Equals|0}
                _dcsbiosOutput = new DCSBIOSOutput();
                _dcsbiosOutput.ImportString(parameters[2]);
            }
            if (settings.StartsWith("MultiPanelDCSBIOSControlLCD{") && settings.Contains("DCSBiosOutputFormula{"))
            {
                //MultiPanelDCSBIOSControlLCD{ALT}\o/{UpperLCD}\o/DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //[0]
                //MultiPanelDCSBIOSFormulaLCD{ALT}
                var param0 = parameters[0].Replace("MultiPanelDCSBIOSControlLCD{", "").Replace("}", "").Trim();
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                //[1]
                //{UpperLCD}
                var param1 = parameters[1].Replace("{", "").Replace("}", "").Trim();
                _pz70LCDPosition = (PZ70LCDPosition)Enum.Parse(typeof(PZ70LCDPosition), param1);

                //[2]
                //DCSBiosOutputFormula{ANT_EGIHQTOD+10}
                _dcsbiosOutputFormula = new DCSBIOSOutputFormula();
                _dcsbiosOutputFormula.ImportString(parameters[2]);
            }
        }

        public PZ70DialPosition DialPosition
        {
            get { return _pz70DialPosition; }
            set { _pz70DialPosition = value; }
        }

        public int CurrentValue
        {
            get => _currentValue;
            set => _currentValue = value;
        }

        public DCSBIOSOutput DCSBIOSOutputObject
        {
            get { return _dcsbiosOutput; }
            set
            {
                _dcsbiosOutput = value;
                _dcsbiosOutputFormula = null;
            }
        }

        public DCSBIOSOutputFormula DCSBIOSOutputFormulaObject
        {
            get { return _dcsbiosOutputFormula; }
            set
            {
                _dcsbiosOutputFormula = value;
                _dcsbiosOutput = null;
            }
        }


        public string ExportSettings()
        {
            if (DCSBIOSOutputObject == null && DCSBIOSOutputFormulaObject == null)
            {
                return null;
            }
            if (_dcsbiosOutputFormula != null)
            {
                //MultiPanelDCSBIOSControlLCD{ALT}\o/{UpperLCDLeft}\o/DCSBiosOutput{ALT_MSL_FT|Equals|0}
                return "MultiPanelDCSBIOSControlLCD{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SeparatorChars + "{" + _pz70LCDPosition + "}" + SeparatorChars + _dcsbiosOutputFormula.ToString();
            }
            return "MultiPanelDCSBIOSControlLCD{" + Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition) + "}" + SeparatorChars + "{" + _pz70LCDPosition + "}" + SeparatorChars + _dcsbiosOutput.ToString();
        }

        public PZ70LCDPosition PZ70LCDPosition
        {
            get { return _pz70LCDPosition; }
            set { _pz70LCDPosition = value; }
        }

        public bool HasBinding
        {
            get { return _dcsbiosOutput != null || _dcsbiosOutputFormula != null; }
        }

        public bool UseFormula
        {
            get { return _dcsbiosOutputFormula != null; }
        }
    }
}
