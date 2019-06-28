using NonVisuals.Radio;

namespace NonVisuals
{
    public class PanelKeyOnOff
    {
    }
    
    public class SwitchPanelPZ55KeyOnOff : PanelKeyOnOff
    {
        private readonly SwitchPanelPZ55Keys _switchPanelPZ55Key;

        public SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys switchPanelPZ55Key, bool buttonState)
        {
            _switchPanelPZ55Key = switchPanelPZ55Key;
            ButtonState = buttonState;
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key => _switchPanelPZ55Key;

        public bool ButtonState { get; }
    }


    public class RadioPanelPZ69KeyOnOff : PanelKeyOnOff
    {
        private readonly RadioPanelPZ69KnobsEmulator _radioPanelPZ69Key;
        private readonly bool _buttonState;

        public RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator radioPanelPZ69Key, bool buttonState)
        {
            _radioPanelPZ69Key = radioPanelPZ69Key;
            _buttonState = buttonState;
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Key => _radioPanelPZ69Key;

        public bool ButtonState => _buttonState;
    }

    public class TPMPanelSwitchOnOff : PanelKeyOnOff
    {
        private readonly TPMPanelSwitches _tpmPanelSwitch;
        private readonly bool _buttonState;

        public TPMPanelSwitchOnOff(TPMPanelSwitches tpmPanelSwitch, bool buttonState)
        {
            _tpmPanelSwitch = tpmPanelSwitch;
            _buttonState = buttonState;
        }

        public TPMPanelSwitches TPMSwitch => _tpmPanelSwitch;

        public bool ButtonState => _buttonState;
    }


    public class MultiPanelPZ70KnobOnOff : PanelKeyOnOff
    {
        private readonly MultiPanelPZ70Knobs _multiPanelPZ70Knob;
        private readonly bool _buttonState;

        public MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs multiPanelPZ70Knobs, bool buttonState)
        {
            _multiPanelPZ70Knob = multiPanelPZ70Knobs;
            _buttonState = buttonState;
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob => _multiPanelPZ70Knob;

        public bool ButtonState => _buttonState;
    }
}
