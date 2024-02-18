using System;
using ClassLibraryCommon;
using MEF;

namespace NonVisuals.BindingClasses.OSCommand
{
    [Serializable]
    [SerializeCriticalCustom]
    public class OSCommandBindingPZ69Emulator : OSCommandBindingBase
    {
        /*
         This class binds a physical switch on the PZ69 key emulator with a Windows OS command.
         */
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;


        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            if (settings.StartsWith("RadioPanelOSPZ69{"))
            {
                var result = ParseSettingV1(settings);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), result.Item2);
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

            return GetExportString("RadioPanelOSPZ69", null, Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key));
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Key
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }

    }
}
