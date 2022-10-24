﻿namespace NonVisuals.DCSBIOSBindings
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DCS_BIOS;

    using MEF;

    using NonVisuals.Saitek;

    [Serializable]
    public class DCSBIOSActionBindingFarmingPanel : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the Farming Simulator Side Panel with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private FarmingPanelMKKeys _farmingPanelKey;




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
                throw new ArgumentException("Import string empty. (DCSBIOSActionBindingFarmingPanel)");
            }
            
            if (settings.StartsWith("FarmingPanelDCSBIOSControl"))
            {
                var skeleton = ParseSetting(settings);
                _farmingPanelKey = (FarmingPanelMKKeys)Enum.Parse(typeof(FarmingPanelMKKeys), skeleton.KeyName);
                /*
                 * Other settings already added.
                 */
            }
        }

        public override string ExportSettings()
        {
            if (DCSBIOSInputs.Count == 0)
            {
                return null;
            }
            
            return GetExportString("FarmingPanelDCSBIOSControlV2", null, Enum.GetName(typeof(SwitchPanelPZ55Keys), FarmingPanelKey));
        }

        public FarmingPanelMKKeys FarmingPanelKey
        {
            get => _farmingPanelKey;
            set => _farmingPanelKey = value;
        }
    }
}
