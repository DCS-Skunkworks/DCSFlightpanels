namespace NonVisuals.CockpitMaster.Panels
{
    using System;
    using ClassLibraryCommon;

    using HidLibrary;
    using HID;
    using NonVisuals.Panels;

    public abstract class CockpitMasterPanel : GamingPanel, IDisposable
    {

        protected readonly object LockLCDUpdateObject = new();
        protected readonly HidDevice HIDWriteDevice;

        protected CockpitMasterPanel(GamingPanelEnum typeOfGamingPanel, HIDSkeleton hidSkeleton) : base(typeOfGamingPanel, hidSkeleton)
        {
            hidSkeleton.PanelHasBeenInstantiated = true;
            HIDWriteDevice = hidSkeleton.HIDWriteDevice;
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
    }
}
