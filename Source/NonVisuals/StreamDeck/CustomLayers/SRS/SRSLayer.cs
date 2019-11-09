using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using NonVisuals.Radios;

namespace NonVisuals.StreamDeck.CustomLayers.SRS
{

    public enum EnumSRSButtonType
    {
        IncrementWholeNumber = 0,
        IncrementDecimalNumber = 1,
        DecrementWholeNumber = 2,
        DecrementDecimalNumber = 3,
        Cycle = 4,
        SelectPreviousRadio = 5,
        SelectNextRadio = 6,
        IncrementChannel = 7,
        DecrementChannel = 8,
        ShowHundredsTensFrequency = 9,
        ShowOnesFrequency = 10,
        ShowTenthsFrequency = 11,
        ShowHundredthsFrequency = 12,
        SetChannel1 = 20,
        SetChannel2 = 21,
        SetChannel3 = 22,
        SetChannel4 = 23,
        SetChannel5 = 24,
        SetChannel6 = 25,
        SetChannel7 = 26,
        SetChannel8 = 27,
        SetChannel9 = 28,
        SetChannel10 = 29
    }

    public class SRSLayer : StreamDeckLayer, ISRSDataListener
    {
        private double _lowerMainFreq = 0;
        private double _upperGuardFreq = 0;
        private double _lowerGuardFreq = 0;
        private int _portFrom;
        private string _ipAddressTo;
        private int _portTo;
        private bool _initialized = false;
        private int _radio;
        private double _radioFrequency;
        private double _guardFrequency;
        private double _channel;
        private SRSRadioMode _srsRadioMode;
        private ButtonFunctionList _buttonFunctionList = new ButtonFunctionList();
        private bool _guardIsOn;


        public SRSLayer()
        {
            var button = new StreamDeckButton(EnumStreamDeckButtonNames.BUTTON1);
            //button
        }

        public void Initialize()
        {
            if (SRSListenerFactory.IsRunning)
            {
                return;
            }
            SRSListenerFactory.SetParams(_portFrom, _ipAddressTo, _portTo);
            SRSListenerFactory.GetSRSListener().Attach(this);
        }

        public int Radio
        {
            get => _radio;
            set => _radio = value;
        }

        public double Frequency
        {
            get
            {
                if (GuardIsOn)
                {
                    return _guardFrequency;
                }
                return _radioFrequency;
            }
            set
            {
                if (GuardIsOn)
                {
                    _guardFrequency = value;
                }
                else
                {
                    _radioFrequency = value;
                }
                    
            }
        }

        public double Channel
        {
            get => _channel;
            set => _channel = value;
        }

        public SRSRadioMode SRSRadioMode
        {
            get => _srsRadioMode;
            set => _srsRadioMode = value;
        }

        public bool GuardIsOn
        {
            get => _guardIsOn;
            set => _guardIsOn = value;
        }

        public void SRSDataReceived(object sender)
        {
            try
            {
                _radioFrequency = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_radio);
                _guardFrequency = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(_radio, true);
                //ShowFrequenciesOnPanel();
            }
            catch (Exception e)
            {
                Common.LogError(8159006, e);
            }
        }

        public new void AddButton(EnumStreamDeckButtonNames streamDeckButtonName, ButtonFunction buttonFunction)
        {

            //StreamDeckButtons.Add();
        }
        
        public new ButtonFunctionList GetCustomButtonList()
        {
            if (_buttonFunctionList.Count > 0)
            {
                return _buttonFunctionList;
            }
            var buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.Cycle, "Toggle guard / manual frequency");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.IncrementChannel, "Goto next higher channel");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.DecrementChannel, "Goto next lower channel");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.IncrementDecimalNumber, "Increment frequency decimal part");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.IncrementWholeNumber, "Increment frequency integer part");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.DecrementDecimalNumber, "Decrement frequency decimal part");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.DecrementWholeNumber, "Decrement frequency integer part");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SelectNextRadio, "Goto next radio");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SelectPreviousRadio, "Goto previous radio");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel1, "Select Channel #1");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel2, "Select Channel #2");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel3, "Select Channel #3");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel4, "Select Channel #4");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel5, "Select Channel #5");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel6, "Select Channel #6");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel7, "Select Channel #7");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel8, "Select Channel #8");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel9, "Select Channel #9");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.SetChannel10, "Select Channel #10");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.ShowHundredsTensFrequency, "Show Hundreds and Tens Frequency 12 (127.495)");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.ShowOnesFrequency, "Show Ones Frequency 7 (127.495)");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.ShowTenthsFrequency, "Show Tenths Frequency 4 (127.495)");
            _buttonFunctionList.Add(buttonFunction);
            buttonFunction = new ButtonFunction((uint)EnumSRSButtonType.ShowHundredthsFrequency, "Show Hundredth Frequency 95 (127.495)");
            _buttonFunctionList.Add(buttonFunction);
            //buttonFunction = new ButtonFunction((uint)EnumSRSButtonType., "");
            //_buttonFunctionList.Add(buttonFunction);
            return _buttonFunctionList;
        }
    }
}
