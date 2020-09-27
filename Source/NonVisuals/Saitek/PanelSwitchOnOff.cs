
 using NonVisuals.Radios;
 using NonVisuals.Radios.Knobs;
 using NonVisuals.Saitek.Switches;
 using NonVisuals.StreamDeck;


 namespace NonVisuals.Saitek
{
    public class PanelSwitchOnOff
    {
    }
    
    public class PZ55SwitchOnOff : PanelSwitchOnOff
    {
        private readonly SwitchPanelPZ55Keys _switchPanelPZ55Key;

        public PZ55SwitchOnOff(SwitchPanelPZ55Keys switchPanelPZ55Key, bool buttonState)
        {
            _switchPanelPZ55Key = switchPanelPZ55Key;
            ButtonState = buttonState;
        }

        public SwitchPanelPZ55Keys Switch => _switchPanelPZ55Key;

        public bool ButtonState { get; }
    }


    public class PZ69SwitchOnOff : PanelSwitchOnOff
    {
        private readonly RadioPanelPZ69KnobsEmulator _radioPanelPZ69Key;
        private readonly bool _buttonState;

        public PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator radioPanelPZ69Key, bool buttonState)
        {
            _radioPanelPZ69Key = radioPanelPZ69Key;
            _buttonState = buttonState;
        }

        public RadioPanelPZ69KnobsEmulator Switch => _radioPanelPZ69Key;

        public bool ButtonState => _buttonState;
    }

    public class TPMSwitchOnOff : PanelSwitchOnOff
    {
        private readonly TPMPanelSwitches _tpmPanelSwitch;
        private readonly bool _buttonState;

        public TPMSwitchOnOff(TPMPanelSwitches tpmPanelSwitch, bool buttonState)
        {
            _tpmPanelSwitch = tpmPanelSwitch;
            _buttonState = buttonState;
        }

        public TPMPanelSwitches Switch => _tpmPanelSwitch;

        public bool ButtonState => _buttonState;
    }


    public class PZ70SwitchOnOff : PanelSwitchOnOff
    {
        private readonly MultiPanelPZ70Knobs _multiPanelPZ70Knob;
        private readonly bool _buttonState;

        public PZ70SwitchOnOff(MultiPanelPZ70Knobs multiPanelPZ70Knobs, bool buttonState)
        {
            _multiPanelPZ70Knob = multiPanelPZ70Knobs;
            _buttonState = buttonState;
        }

        public MultiPanelPZ70Knobs Switch => _multiPanelPZ70Knob;

        public bool ButtonState => _buttonState;
    }


    public class StreamDeckButtonOnOff : PanelSwitchOnOff
    {
        private readonly EnumStreamDeckButtonNames _enumStreamDeckButtonName;
        private readonly bool _buttonState;

        public StreamDeckButtonOnOff(EnumStreamDeckButtonNames enumStreamDeckButton, bool buttonState)
        {
            _enumStreamDeckButtonName = enumStreamDeckButton;
            _buttonState = buttonState;
        }

        public EnumStreamDeckButtonNames EnumStreamDeckButton => _enumStreamDeckButtonName;

        public bool ButtonState => _buttonState;
    }
}
