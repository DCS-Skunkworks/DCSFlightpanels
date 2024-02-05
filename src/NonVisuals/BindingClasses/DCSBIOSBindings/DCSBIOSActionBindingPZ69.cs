using System;
using MEF;

namespace NonVisuals.BindingClasses.DCSBIOSBindings
{
    [Serializable]
    public class DCSBIOSActionBindingPZ69 : DCSBIOSActionBindingBase
    {
        /*
         This class binds a physical switch on the PZ69 with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private PZ69DialPosition _pz69DialPosition;
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;



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
                throw new ArgumentException("Import string empty. (DCSBIOSBindingPZ69)");
            }

            if (settings.StartsWith("RadioPanelDCSBIOSControl"))
            {
                var result = ParseSetting(settings);
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), result.Item1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), result.Item2);
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

            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in DCSBIOSBindingPZ69 for knob " + RadioPanelPZ69Knob + ". Cannot export.");
            }

            return GetExportString("RadioPanelDCSBIOSControlV2", Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition), Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob));
        }

        public PZ69DialPosition DialPosition
        {
            get => _pz69DialPosition;
            set => _pz69DialPosition = value;
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Knob
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }
    }
}
