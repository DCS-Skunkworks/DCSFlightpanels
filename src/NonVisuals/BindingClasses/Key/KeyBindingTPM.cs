using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using MEF;

namespace NonVisuals.BindingClasses.Key
{
    [Serializable]
    [SerializeCriticalCustom]
    public class KeyBindingTPM : KeyBindingBase
    {
        /*
         This class binds a physical switch on the TPM with a user made virtual keypress in Windows.
         */
        private TPMPanelSwitches _tpmPanelSwitch;


        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            if (settings.StartsWith("SwitchPanelKey{"))
            {
                var result = ParseSettingV1(settings);

                TPMSwitch = (TPMPanelSwitches)Enum.Parse(typeof(TPMPanelSwitches), result.Item2);
                /*
                 * All others settings set already
                 */
            }
        }

        public override string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }

            return GetExportString("TPMPanelSwitch", null, Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch));
        }


        public TPMPanelSwitches TPMSwitch
        {
            get => _tpmPanelSwitch;
            set => _tpmPanelSwitch = value;
        }

        public static HashSet<KeyBindingTPM> SetNegators(HashSet<KeyBindingTPM> knobBindings)
        {
            if (knobBindings == null)
            {
                return null;
            }

            foreach (var keyBindingTPM in knobBindings)
            {
                /*
                 * Some deleted keystrokes may be included here, they have a wrapper although the actual OSKeyPress is null
                 * and will be removed until next "Save". So disregard those.
                 */
                if (keyBindingTPM.OSKeyPress == null)
                {
                    continue;
                }

                // Clear all negators
                keyBindingTPM.OSKeyPress.NegatorOSKeyPresses.Clear();

                foreach (var keyBinding in knobBindings)
                {
                    if (keyBinding != keyBindingTPM && keyBinding.TPMSwitch == keyBindingTPM.TPMSwitch && keyBinding.WhenTurnedOn != keyBindingTPM.WhenTurnedOn)
                    {
                        keyBindingTPM.OSKeyPress.NegatorOSKeyPresses.Add(keyBinding.OSKeyPress);
                    }
                }
            }

            return knobBindings;
        }
    }
}
