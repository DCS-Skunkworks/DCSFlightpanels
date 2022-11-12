using System;
using MEF;
using NonVisuals.Saitek.Panels;

namespace NonVisuals.BindingClasses.DCSBIOSBindings
{
    [Serializable]
    public class DCSBIOSActionBindingPZ70 : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the PZ70 with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private PZ70DialPosition _pz70DialPosition;
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;





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
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ70)");
            }

            if (settings.StartsWith("MultiPanelDCSBIOSControl"))
            {
                var result = ParseSetting(settings);
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), result.Item1);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), result.Item2);
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

            return GetExportString("MultiPanelDCSBIOSControlV2", Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition), Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob));
        }

        public PZ70DialPosition DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get => _multiPanelPZ70Knob;
            set => _multiPanelPZ70Knob = value;
        }

    }
}
