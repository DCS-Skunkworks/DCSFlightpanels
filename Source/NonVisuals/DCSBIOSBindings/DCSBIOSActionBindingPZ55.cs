namespace NonVisuals.DCSBIOSBindings
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DCS_BIOS;

    using MEF;

    using NonVisuals.Saitek;

    [Serializable]
    public class DCSBIOSActionBindingPZ55 : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the PZ55 with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private SwitchPanelPZ55Keys _switchPanelPZ55Key;




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
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ55)");
            }
            
            if (settings.StartsWith("SwitchPanelDCSBIOSControlV2{"))
            {
                var skeleton = ParseSetting(settings);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), skeleton.KeyName);
                /*
                 * Other settings already added.
                 */
            }
            else if (settings.StartsWith("SwitchPanelDCSBIOSControl{")) // Older settings style without config int
            {
                var skeleton = ParseSetting(settings);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), skeleton.KeyName);
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

            var onStr = WhenOnTurnedOn ? "1" : "0";

            // \o/DCSBIOSInput{AAP_STEER|SET_STATE|2}\o/DCSBIOSInput{BAT_PWR|INC|2}
            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in DCSBIOSInputs)
            {
                stringBuilder.Append(SaitekConstants.SEPARATOR_SYMBOL + dcsbiosInput);
            }

            if (!string.IsNullOrWhiteSpace(Description))
            {
                return "SwitchPanelDCSBIOSControlV2{" + GetSettingsInt() + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{"  + onStr + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "|" + Description + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder;
            }

            return "SwitchPanelDCSBIOSControlV2{" + GetSettingsInt() + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "}" + SaitekConstants.SEPARATOR_SYMBOL + stringBuilder;
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }
    }
}
