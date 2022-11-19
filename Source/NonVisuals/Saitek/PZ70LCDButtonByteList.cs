namespace NonVisuals.Saitek
{
    using System;

    using MEF;
    using NLog;
    using Panels;


    /*
     * Class for handling the buttons below the LCD of the Multi Panel (PZ70).
     */
    public class PZ70LCDButtonByteList
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        /*
            LCD Button Byte
            00000000
            ||||||||_ AP_BUTTON
            |||||||_ HDG_BUTTON
            ||||||_ NAV_BUTTON
            |||||_ IAS_BUTTON
            ||||_ ALT_BUTTON
            |||_ VS_BUTTON
            ||_ APR_BUTTON
            |_ REV_BUTTON
        */
        private const byte AP_MASK = 1;
        private const byte HDG_MASK = 2;
        private const byte NAV_MASK = 4;
        private const byte IAS_MASK = 8;
        private const byte ALT_MASK = 16;
        private const byte VS_MASK = 32;
        private const byte APR_MASK = 64;
        private const byte REV_MASK = 128;
        private const int DIAL_ALT_MASK = 256;
        private const int DIAL_VS_MASK = 512;
        private const int DIAL_IAS_MASK = 1024;
        private const int DIAL_HDG_MASK = 2048;
        private const int DIAL_CRS_MASK = 4096;
        private const int BUTTON_IS_ON_MASK = 8192;

        // bool isSet = (b & mask) != 0
        // Set to 1" b |= mask
        // Set to zero
        // b &= ~mask
        // Toggle
        // b ^= mask
        private readonly byte[] _buttonBytes = new byte[8];
        private readonly int[] _buttonDialPosition = new int[8];

        public PZ70LCDButtonByteList()
        {
            _buttonDialPosition[0] |= DIAL_ALT_MASK;
            _buttonDialPosition[1] |= DIAL_VS_MASK;
            _buttonDialPosition[2] |= DIAL_IAS_MASK;
            _buttonDialPosition[3] |= DIAL_HDG_MASK;
            _buttonDialPosition[4] |= DIAL_CRS_MASK;
        }

        public bool FlipButton(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            try
            {
                return FlipButton(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Flipbutton()");
                throw;
            }
        }

        public bool IsOn(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knobs)
        {
            try
            {
                var dialMask = GetMaskForDialPosition(pz70DialPosition);
                var buttonMask = GetMaskForButton(multiPanelPZ70Knobs);
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & dialMask) != 0)
                    {
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }

                throw new Exception("Multipanel IsOn : Failed to find Mask for dial position " + pz70DialPosition + " knob " + multiPanelPZ70Knobs);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "IsOn()");
                throw;
            }
        }

        public static int GetMaskForDialPosition(PZ70DialPosition pz70DialPosition)
        {
            return pz70DialPosition switch
            {
                PZ70DialPosition.ALT => DIAL_ALT_MASK,
                PZ70DialPosition.VS  => DIAL_VS_MASK,
                PZ70DialPosition.IAS => DIAL_IAS_MASK,
                PZ70DialPosition.HDG => DIAL_HDG_MASK,
                PZ70DialPosition.CRS => DIAL_CRS_MASK,
                _ => throw new Exception($"Multipanel : Failed to find Mask for dial position {pz70DialPosition}")
            };
        }

        public static byte GetMaskForButton(MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            return multiPanelPZ70Knob switch
            {
                MultiPanelPZ70Knobs.AP_BUTTON  => AP_MASK,
                MultiPanelPZ70Knobs.HDG_BUTTON => HDG_MASK,
                MultiPanelPZ70Knobs.NAV_BUTTON => NAV_MASK,
                MultiPanelPZ70Knobs.IAS_BUTTON => IAS_MASK,
                MultiPanelPZ70Knobs.ALT_BUTTON => ALT_MASK,
                MultiPanelPZ70Knobs.VS_BUTTON  => VS_MASK,
                MultiPanelPZ70Knobs.APR_BUTTON => APR_MASK,
                MultiPanelPZ70Knobs.REV_BUTTON => REV_MASK,
                _ => throw new Exception($"Multipanel : Failed to find Mask for button {multiPanelPZ70Knob}")
            };
        }

        public bool FlipButton(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] ^= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }

                throw new Exception("Multipanel FlipButton : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public void SetButtonOff(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            SetButtonOff(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob));
        }

        public bool SetButtonOff(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] &= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }

                throw new Exception("Multipanel ButtonOff : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public byte GetButtonByte(PZ70DialPosition pz70DialPosition)
        {
            try
            {
                var mask = GetMaskForDialPosition(pz70DialPosition);
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    // (b & mask) != 0
                    if ((_buttonDialPosition[i] & mask) != 0)
                    {
                        return _buttonBytes[i];
                    }
                }

                throw new Exception("Multipanel : Failed to find button byte for " + pz70DialPosition);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }
    }
}
