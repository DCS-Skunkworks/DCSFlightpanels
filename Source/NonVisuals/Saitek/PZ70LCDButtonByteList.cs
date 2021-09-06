using System;
using ClassLibraryCommon;
using NonVisuals.Saitek.Panels;

namespace NonVisuals.Saitek
{
    using MEF;

    public class PZ70LCDButtonByteList
    {
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

        //bool isSet = (b & mask) != 0
        //Set to 1" b |= mask
        //Set to zero
        //b &= ~mask
        //Toggle
        //b ^= mask
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
            catch (Exception e)
            {
                Common.LogError(e, "Flipbutton()");
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
            catch (Exception e)
            {
                Common.LogError(e, "IsOn()");
                throw;
            }
        }

        public int GetMaskForDialPosition(PZ70DialPosition pz70DialPosition)
        {
            try
            {

                switch (pz70DialPosition)
                {
                    case PZ70DialPosition.ALT:
                        {
                            return DIAL_ALT_MASK;
                        }
                    case PZ70DialPosition.VS:
                        {
                            return DIAL_VS_MASK;
                        }
                    case PZ70DialPosition.IAS:
                        {
                            return DIAL_IAS_MASK;
                        }
                    case PZ70DialPosition.HDG:
                        {
                            return DIAL_HDG_MASK;
                        }
                    case PZ70DialPosition.CRS:
                        {
                            return DIAL_CRS_MASK;
                        }
                }
                throw new Exception("Multipanel : Failed to find Mask for dial position " + pz70DialPosition);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetMaskForButton(MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            try
            {
                switch (multiPanelPZ70Knob)
                {
                    case MultiPanelPZ70Knobs.AP_BUTTON:
                        {
                            return AP_MASK;
                        }
                    case MultiPanelPZ70Knobs.HDG_BUTTON:
                        {
                            return HDG_MASK;
                        }
                    case MultiPanelPZ70Knobs.NAV_BUTTON:
                        {
                            return NAV_MASK;
                        }
                    case MultiPanelPZ70Knobs.IAS_BUTTON:
                        {
                            return IAS_MASK;
                        }
                    case MultiPanelPZ70Knobs.ALT_BUTTON:
                        {
                            return ALT_MASK;
                        }
                    case MultiPanelPZ70Knobs.VS_BUTTON:
                        {
                            return VS_MASK;
                        }
                    case MultiPanelPZ70Knobs.APR_BUTTON:
                        {
                            return APR_MASK;
                        }
                    case MultiPanelPZ70Knobs.REV_BUTTON:
                        {
                            return REV_MASK;
                        }
                }
                throw new Exception("Multipanel : Failed to find Mask for button " + multiPanelPZ70Knob);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
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
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public void SetButtonOnOrOff(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knob, bool on)
        {
            SetButtonOnOrOff(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob), on);
        }

        public bool SetButtonOnOrOff(int buttonDialMask, byte buttonMask, bool on)
        {
            if (on)
            {
                return SetButtonOn(buttonDialMask, buttonMask);
            }

            return SetButtonOff(buttonDialMask, buttonMask);
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
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public void SetButtonOn(PZ70DialPosition pz70DialPosition, MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            SetButtonOn(GetMaskForDialPosition(pz70DialPosition), GetMaskForButton(multiPanelPZ70Knob));
        }

        public bool SetButtonOn(int buttonDialMask, byte buttonMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        _buttonBytes[i] |= buttonMask;
                        return (_buttonBytes[i] & buttonMask) != 0;
                    }
                }
                throw new Exception("Multipanel ButtonOn : Failed to find Mask for dial " + buttonDialMask + " button " + buttonMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
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
                    //(b & mask) != 0
                    if ((_buttonDialPosition[i] & mask) != 0)
                    {
                        return _buttonBytes[i];
                    }
                }
                throw new Exception("Multipanel : Failed to find button byte for " + pz70DialPosition);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        public byte GetButtonByte(int buttonDialMask)
        {
            try
            {
                for (int i = 0; i < _buttonDialPosition.Length; i++)
                {
                    if ((_buttonDialPosition[i] & buttonDialMask) != 0)
                    {
                        return _buttonBytes[i];
                    }
                }
                throw new Exception("Multipanel GetButtonByte : Failed to find Mask for dial " + buttonDialMask);
            }
            catch (Exception e)
            {
                Common.LogError(e);
                throw;
            }
        }

        


    }
}
