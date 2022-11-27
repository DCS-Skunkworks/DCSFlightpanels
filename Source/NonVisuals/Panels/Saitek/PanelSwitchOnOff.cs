namespace NonVisuals.Panels.Saitek
{
    using MEF;

    public class PanelSwitchOnOff
    {
    }

    /// <summary>
    /// Each panel has its own set of switches and knobs and buttons.
    /// All switches have(so far) an On or Off mode, knobs that can
    /// be turned have "Increase" or "Decrease".
    /// This is used by the UI, each TextBox is used for one of these
    /// positions(On/Off/Increase/Decrease) for a certain switch/knob/button.
    /// So when the user sets a value for the Switch Panel's MASTER BAT and OFF
    /// the class contains this information.When the new setting the user has
    /// entered is passed on to the panel instance(responsible for saving the settings)
    /// this class is passed along with the actual setting the user chose such as
    /// key emulation or dcs-bios etc.
    /// So the button ID & On/Off state + actual setting (key/dcs-bios) is passed on to the panel.
    /// </summary>
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

    public class FarmingPanelOnOff : PanelSwitchOnOff
    {
        private readonly FarmingPanelMKKeys _farmingPanelMKKey;

        public FarmingPanelOnOff(FarmingPanelMKKeys farmingPanelKey, bool buttonState)
        {
            _farmingPanelMKKey = farmingPanelKey;
            ButtonState = buttonState;
        }

        public FarmingPanelMKKeys Switch => _farmingPanelMKKey;

        public bool ButtonState { get; }
    }

}
