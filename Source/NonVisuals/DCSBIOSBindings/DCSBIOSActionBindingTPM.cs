namespace NonVisuals.DCSBIOSBindings
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DCS_BIOS;

    using MEF;

    using NonVisuals.Saitek;

    [Serializable]
    public class DCSBIOSActionBindingTPM : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the TPM with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private TPMPanelSwitches _tpmPanelSwitch;





        private bool _disposed;
        // Protected implementation of Dispose pattern.
        public override void Dispose(bool disposing)
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

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingTPM)");
            }
            
            if (settings.StartsWith("TPMPanelDCSBIOSControl"))
            {
                var skeleton = ParseSetting(settings);
                _tpmPanelSwitch = (TPMPanelSwitches)Enum.Parse(typeof(TPMPanelSwitches), skeleton.KeyName);
                /*
                 * Other settings already added.
                 */
            }
        }
        
        public TPMPanelSwitches TPMSwitch
        {
            get => _tpmPanelSwitch;
            set => _tpmPanelSwitch = value;
        }

        public override string ExportSettings()
        {
            if (DCSBIOSInputs.Count == 0)
            {
                return null;
            }
            
            return GetExportString("TPMPanelDCSBIOSControlV2", null, Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch));
        }

    }
}
