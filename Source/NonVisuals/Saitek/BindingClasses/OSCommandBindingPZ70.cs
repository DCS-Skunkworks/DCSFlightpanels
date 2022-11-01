namespace NonVisuals.Saitek.BindingClasses
{
    using System;

    using MEF;

    using NonVisuals.Saitek.Panels;

    [Serializable]
    public class OSCommandBindingPZ70 : OSCommandBindingBase
    {
        /*
         This class binds a physical switch on the PZ70 with a Windows OS command.
         */
        private PZ70DialPosition _pz70DialPosition;
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;


        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (WindowsBinding)");
            }

            if (settings.StartsWith("MultiPanelOSPZ70{"))
            {
                var result = ParseSettingV1(settings);
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), result.Item1);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), result.Item2);
                /*
                 * other settings already added
                 */
            }
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }

            return GetExportString("MultiPanelOSPZ70", Enum.GetName(typeof(PZ70DialPosition), _pz70DialPosition), Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob));
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
