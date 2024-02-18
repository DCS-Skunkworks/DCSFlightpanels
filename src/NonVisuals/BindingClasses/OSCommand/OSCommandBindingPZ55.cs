using System;
using ClassLibraryCommon;
using MEF;
using NonVisuals.Panels.Saitek;

namespace NonVisuals.BindingClasses.OSCommand
{
    [Serializable]
    [SerializeCriticalCustom]
    public class OSCommandBindingPZ55 : OSCommandBindingBase
    {
        /*
         This class binds a physical switch on the PZ55 with a Windows OS command.
         */
        private SwitchPanelPZ55Keys _switchPanelPZ55Key;

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (WindowsBinding)");
            }

            if (settings.StartsWith("SwitchPanelOSPZ55{"))
            {
                // SwitchPanelOSPZ55{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // SwitchPanelOSPZ55{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("SwitchPanelOSPZ55{", string.Empty).Replace("}", string.Empty);

                // 1KNOB_ENGINE_LEFT
                WhenTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _switchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), param0);

                // OSCommand{FILE\o/ARGUMENTS\o/NAME}
                OSCommandObject = new NonVisuals.OSCommand();
                OSCommandObject.ImportString(parameters[1]);
            }
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }

            return GetExportString("SwitchPanelOSPZ55", null, Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key));
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get => _switchPanelPZ55Key;
            set => _switchPanelPZ55Key = value;
        }
    }
}
