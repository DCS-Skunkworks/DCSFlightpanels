using System;
using ClassLibraryCommon;
using MEF;

namespace NonVisuals.BindingClasses.DCSBIOSBindings
{
    [Serializable]
    [SerializeCriticalCustom]
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
            
            if (settings.StartsWith("SwitchPanelDCSBIOSControl"))
            {
                var result = ParseSetting(settings);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), result.Item2);
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

            return GetExportString("SwitchPanelDCSBIOSControlV2", null, Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key));
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }
    }
}
