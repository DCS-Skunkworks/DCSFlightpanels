using NonVisuals.Interfaces;
using NonVisuals.Radios;

namespace NonVisuals.StreamDeck.CustomLayers.SRS
{
    public class SRSAction : IStreamDeckButtonAction
    {
        private readonly string _description;
        private EnumSRSButtonType _functionType;
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.Custom;
        public int RadioNumber { get; set; }
        public string Description => _description;
        public ISRSHandler SRSHandler { get; set; }
        private bool _isRepeatable;

        public SRSAction(string description, EnumSRSButtonType functionType, ISRSHandler srsHandler)
        {
            _description = description;
            _functionType = functionType;
            SRSHandler = srsHandler;
        }

        public void Execute(StreamDeckRequisites streamDeckRequisites)
        {
            switch (FunctionType)
            {
                case EnumSRSButtonType.Cycle:
                    {
                        SRSListenerFactory.GetSRSListener().ToggleGuard(RadioNumber);
                        break;
                    }
                case EnumSRSButtonType.SelectNextRadio:
                    {
                        RadioNumber++;
                        if (RadioNumber > 7)
                        {
                            RadioNumber = 1;
                        }

                        UpdateSRSFrequency();
                        break;
                    }
                case EnumSRSButtonType.SelectPreviousRadio:
                    {
                        RadioNumber--;
                        if (RadioNumber < 0)
                        {
                            RadioNumber = 7;
                        }

                        UpdateSRSFrequency();
                        break;
                    }
                case EnumSRSButtonType.IncrementWholeNumber:
                    {
                        SRSHandler.Frequency++;
                        break;
                    }
                case EnumSRSButtonType.DecrementWholeNumber:
                    {
                        SRSHandler.Frequency--;
                        break;
                    }
                case EnumSRSButtonType.IncrementDecimalNumber:
                    {
                        SRSHandler.Frequency += 0.25;
                        break;
                    }
                case EnumSRSButtonType.DecrementDecimalNumber:
                    {
                        SRSHandler.Frequency -= 0.25;
                        break;
                    }
                case EnumSRSButtonType.IncrementChannel:
                    {
                        SRSHandler.Channel++;
                        break;
                    }
                case EnumSRSButtonType.DecrementChannel:
                    {
                        SRSHandler.Channel--;
                        break;
                    }
                case EnumSRSButtonType.SetChannel1:
                    {
                        SRSHandler.Channel = 1;
                        break;
                    }
                case EnumSRSButtonType.SetChannel2:
                    {
                        SRSHandler.Channel = 2;
                        break;
                    }
                case EnumSRSButtonType.SetChannel3:
                    {
                        SRSHandler.Channel = 3;
                        break;
                    }
                case EnumSRSButtonType.SetChannel4:
                    {
                        SRSHandler.Channel = 4;
                        break;
                    }
                case EnumSRSButtonType.SetChannel5:
                    {
                        SRSHandler.Channel = 5;
                        break;
                    }
                case EnumSRSButtonType.SetChannel6:
                    {
                        SRSHandler.Channel = 6;
                        break;
                    }
                case EnumSRSButtonType.SetChannel7:
                    {
                        SRSHandler.Channel = 7;
                        break;
                    }
                case EnumSRSButtonType.SetChannel8:
                    {
                        SRSHandler.Channel = 8;
                        break;
                    }
                case EnumSRSButtonType.SetChannel9:
                    {
                        SRSHandler.Channel = 9;
                        break;
                    }
                case EnumSRSButtonType.SetChannel10:
                    {
                        SRSHandler.Channel = 10;
                        break;
                    }
                default:
                    {
                        SRSHandler.Channel = 1;
                        break;
                    }
            }
        }

        private void SetRepeatStatus()
        {
            switch (FunctionType)
            {
                case EnumSRSButtonType.Cycle:
                case EnumSRSButtonType.SelectNextRadio:
                case EnumSRSButtonType.SelectPreviousRadio:
                case EnumSRSButtonType.IncrementChannel:
                case EnumSRSButtonType.DecrementChannel:
                case EnumSRSButtonType.SetChannel1:
                case EnumSRSButtonType.SetChannel2:
                case EnumSRSButtonType.SetChannel3:
                case EnumSRSButtonType.SetChannel4:
                case EnumSRSButtonType.SetChannel5:
                case EnumSRSButtonType.SetChannel6:
                case EnumSRSButtonType.SetChannel7:
                case EnumSRSButtonType.SetChannel8:
                case EnumSRSButtonType.SetChannel9:
                case EnumSRSButtonType.SetChannel10:
                    {
                        _isRepeatable = false;
                        break;
                    }
                case EnumSRSButtonType.IncrementWholeNumber:
                case EnumSRSButtonType.DecrementWholeNumber:
                case EnumSRSButtonType.IncrementDecimalNumber:
                case EnumSRSButtonType.DecrementDecimalNumber:
                    {
                        _isRepeatable = true;
                        break;
                    }
                default:
                    {
                        _isRepeatable = false;
                        break;
                    }
            }
        }

        private void UpdateSRSFrequency()
        {
            var mode = SRSListenerFactory.GetSRSListener().GetRadioMode(RadioNumber);
            SRSHandler.RadioMode = mode;
            if (mode == SRSRadioMode.Channel)
            {
                SRSHandler.Channel = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(RadioNumber, SRSHandler.GuardIsOn);
            }
            else
            {
                SRSHandler.Frequency = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(RadioNumber, SRSHandler.GuardIsOn);
            }
        }

        public EnumSRSButtonType FunctionType
        {
            get => _functionType;
            set => _functionType = value;
        }

        public bool IsRunning()
        {
            return false;
        }

        public bool IsRepeatable()
        {
            return _isRepeatable;
        }
    }
}
