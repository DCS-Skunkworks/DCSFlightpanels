namespace NonVisuals.Saitek.Panels
{
    using System;
    using System.Collections.Generic;

    using ClassLibraryCommon;

    using DCS_BIOS;

    using HidLibrary;

    using MEF;

    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Saitek.Switches;

    public abstract class SaitekPanel : GamingPanel, IDisposable
    {
        public abstract void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff);

        public abstract void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength);

        public abstract void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence);

        public abstract void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description);

        public abstract void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink);

        public abstract void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand);

        public abstract DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput);

        protected HashSet<ISaitekPanelKnob> SaitekPanelKnobs = new HashSet<ISaitekPanelKnob>();

        protected byte[] OldSaitekPanelValue = { 0, 0, 0 };

        protected byte[] NewSaitekPanelValue = { 0, 0, 0 };

        protected byte[] OldSaitekPanelValueTPM = { 0, 0, 0, 0, 0 };

        protected byte[] NewSaitekPanelValueTPM = { 0, 0, 0, 0, 0 };

        protected byte[] OldPanelValueFarmingPanel = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        protected byte[] NewPanelValueFarmingPanel = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        protected SaitekPanel(GamingPanelEnum typeOfGamingPanel, HIDSkeleton hidSkeleton) : base(typeOfGamingPanel, hidSkeleton)
        {
            hidSkeleton.PanelHasBeenInstantiated = true;
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        protected override void StartListeningForHidPanelChanges()
        {
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    // Debug.Write("Adding callback " + HIDSkeletonBase.PanelInfo.GamingPanelType\n");
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void OnReport(HidReport report)
        {
            if (TypeOfPanel == GamingPanelEnum.TPM && report.Data.Length == 5)
            {
                Array.Copy(NewSaitekPanelValueTPM, OldSaitekPanelValueTPM, 5);
                Array.Copy(report.Data, NewSaitekPanelValueTPM, 5);
                var hashSet = GetHashSetOfChangedKnobs(OldSaitekPanelValueTPM, NewSaitekPanelValueTPM);
                if (hashSet.Count > 0)
                {
                    GamingPanelKnobChanged(!FirstReportHasBeenRead, hashSet);
                    AppEventHandler.SwitchesChanged(this, HIDSkeletonBase.HIDInstance, TypeOfPanel, hashSet);
                }

                FirstReportHasBeenRead = true;
            }
            else if (report.Data.Length == 3)
            {
                Array.Copy(NewSaitekPanelValue, OldSaitekPanelValue, 3);
                Array.Copy(report.Data, NewSaitekPanelValue, 3);
                var hashSet = GetHashSetOfChangedKnobs(OldSaitekPanelValue, NewSaitekPanelValue);
                if (hashSet.Count > 0)
                {
                    GamingPanelKnobChanged(!FirstReportHasBeenRead, hashSet);
                    AppEventHandler.SwitchesChanged(this, HIDSkeletonBase.HIDInstance, TypeOfPanel, hashSet);
                }

                FirstReportHasBeenRead = true;
            }
            else if (report.Data.Length == 27)
            {
                Array.Copy(NewPanelValueFarmingPanel, OldPanelValueFarmingPanel, 27);
                Array.Copy(report.Data, NewPanelValueFarmingPanel, 27);
                var hashSet = GetHashSetOfChangedKnobs(OldPanelValueFarmingPanel, NewPanelValueFarmingPanel);
                if (hashSet.Count > 0)
                {
                    GamingPanelKnobChanged(!FirstReportHasBeenRead, hashSet);
                    AppEventHandler.SwitchesChanged(this, HIDSkeletonBase.HIDInstance, TypeOfPanel, hashSet);
                }

                FirstReportHasBeenRead = true;
            }

            StartListeningForHidPanelChanges();
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();

            var endValue = 3;
            if(TypeOfPanel == GamingPanelEnum.TPM)
            {
                endValue = 5;
            }
            else if (TypeOfPanel == GamingPanelEnum.FarmingPanel)
            {
                endValue = 4;
            }

            for (var i = 0; i < endValue; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var saitekPanelKnob in SaitekPanelKnobs)
                {
                    if (saitekPanelKnob.Group == i && (FlagHasChanged(oldByte, newByte, saitekPanelKnob.Mask) || !FirstReportHasBeenRead))
                    {
                        var addKnob = true;

                        saitekPanelKnob.IsOn = FlagValue(newValue, saitekPanelKnob);
                        if (saitekPanelKnob.GetType() == typeof(MultiPanelKnob) && !saitekPanelKnob.IsOn)
                        {
                            var multiPanelKnob = (MultiPanelKnob)saitekPanelKnob;
                            switch (multiPanelKnob.MultiPanelPZ70Knob)
                            {
                                case MultiPanelPZ70Knobs.AP_BUTTON:
                                case MultiPanelPZ70Knobs.HDG_BUTTON:
                                case MultiPanelPZ70Knobs.NAV_BUTTON:
                                case MultiPanelPZ70Knobs.IAS_BUTTON:
                                case MultiPanelPZ70Knobs.ALT_BUTTON:
                                case MultiPanelPZ70Knobs.VS_BUTTON:
                                case MultiPanelPZ70Knobs.APR_BUTTON:
                                case MultiPanelPZ70Knobs.REV_BUTTON:
                                    {
                                        /*
                                         * IMPORTANT
                                         * ---------
                                         * The LCD buttons toggle between on and off. It is the toggle value that defines if the button is OFF, not the fact that the user releases the button.
                                         * Therefore the fore-mentioned buttons cannot be used as usual in a loop with knobBinding.WhenTurnedOn
                                         * Instead the buttons global bool value must be used!
                                         * 
                                         */
                                        // Do not add OFF values for these buttons! Read comment above.
                                        addKnob = false;
                                        break;
                                    }
                            }
                        }

                        if (addKnob)
                        {
                            result.Add(saitekPanelKnob);
                        }

                    }
                }
            }

            return result;
        }


        protected static bool FlagHasChanged(byte oldValue, byte newValue, int bitMask)
        {
            /*  --------------------------------------- 
             *  Example #1
             *  Old value 10110101
             *  New value 10110001
             *  Bit mask  00000100  <- This is the one we are interested in to see whether it has changed
             *  ---------------------------------------
             *  
             *  XOR       10110101
             *  ^         10110001
             *            --------
             *            00000100   <- Here are the bit(s) that has changed between old & new value
             *            
             *  AND       00000100
             *  &         00000100
             *            --------
             *            00000100   <- This shows that the value for this mask has changed since last time. Now get what is it (ON/OFF) using FlagValue function
             */

            /*  --------------------------------------- 
             *  Example #2
             *  Old value 10110101
             *  New value 10100101
             *  Bit mask  00000100  <- This is the one we are interested in to see whether it has changed
             *  ---------------------------------------
             *  
             *  XOR       10110101
             *  ^         10100101
             *            --------
             *            00010000   <- Here are the bit(s) that has changed between old & new value
             *            
             *  AND       00010000
             *  &         00000100
             *            --------
             *            00000000   <- This shows that the value for this mask has NOT changed since last time.
             */
            return ((oldValue ^ newValue) & bitMask) > 0;
        }

        private static bool FlagValue(byte[] currentValue, ISaitekPanelKnob saitekPanelKnob)
        {
            return (currentValue[saitekPanelKnob.Group] & saitekPanelKnob.Mask) > 0;
        }
    }


    public enum ControlList : byte
    {
        ALL,
        DCSBIOS,
        KEYS,
        BIPS,
        OSCOMMANDS
    }
}
