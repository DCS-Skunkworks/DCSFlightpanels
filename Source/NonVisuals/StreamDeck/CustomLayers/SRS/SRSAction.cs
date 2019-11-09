using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public SRSLayer ParentSRSLayer { get; set; }

        public void Execute(StreamDeckRequisites streamDeckRequisites)
        {
            switch (FunctionType)
            {
                case EnumSRSButtonType.Cycle:
                    {
                        SRSListenerFactory.GetSRSListener().ToggleBetweenGuardAndFrequency(RadioNumber);
                        break;
                    }
                case EnumSRSButtonType.SelectNextRadio:
                    {
                        RadioNumber++;
                        if (RadioNumber > 7)
                        {
                            RadioNumber = 1;
                        }

                        UpdateParentFrequency();
                        break;
                    }
                case EnumSRSButtonType.SelectPreviousRadio:
                    {
                        RadioNumber--;
                        if (RadioNumber < 0)
                        {
                            RadioNumber = 7;
                        }

                        UpdateParentFrequency();
                        break;
                    }
                case EnumSRSButtonType.IncrementWholeNumber:
                    {
                        break;
                    }
                case EnumSRSButtonType.DecrementWholeNumber:
                    {
                        break;
                    }
                case EnumSRSButtonType.IncrementDecimalNumber:
                    {
                        break;
                    }
                case EnumSRSButtonType.DecrementDecimalNumber:
                    {
                        break;
                    }
                case EnumSRSButtonType.IncrementChannel:
                    {
                        break;
                    }
                case EnumSRSButtonType.DecrementChannel:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel1:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel2:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel3:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel4:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel5:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel6:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel7:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel8:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel9:
                    {
                        break;
                    }
                case EnumSRSButtonType.SetChannel10:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void UpdateParentFrequency()
        {
            var mode = SRSListenerFactory.GetSRSListener().GetRadioMode(RadioNumber);
            ParentSRSLayer.SRSRadioMode = mode;
            if (mode == SRSRadioMode.Channel)
            {
                ParentSRSLayer.Channel = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(RadioNumber, ParentSRSLayer.GuardIsOn);
            }
            else
            {
                ParentSRSLayer.Frequency = SRSListenerFactory.GetSRSListener().GetFrequencyOrChannel(RadioNumber, ParentSRSLayer.GuardIsOn);
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
            return true;
        }
    }
}
