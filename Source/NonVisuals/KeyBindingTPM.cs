﻿using System;

namespace NonVisuals
{
    public class KeyBindingTPM
    {
        /*
         This class binds a physical switch on the TPM with a user made virtual keypress in Windows.
         */
        private TPMPanelSwitches _tpmPanelSwitch;
        private OSKeyPress _osKeyPress;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("TPMPanelSwitch{"))
            {
                //TPMPanelSwitch{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //TPMPanelSwitch{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Trim().Substring(15);
                //1KNOB_ENGINE_LEFT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //1KNOB_ENGINE_LEFT
                _whenOnTurnedOn = (param0.Substring(0, 1) == "1");
                param0 = param0.Substring(1);
                _tpmPanelSwitch = (TPMPanelSwitches)Enum.Parse(typeof(TPMPanelSwitches), param0);

                //OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                _osKeyPress = new OSKeyPress();
                _osKeyPress.ImportString(parameters[1]);
            }
        }

        public TPMPanelSwitches TPMSwitch
        {
            get { return _tpmPanelSwitch; }
            set { _tpmPanelSwitch = value; }
        }

        public OSKeyPress OSKeyPress
        {
            get { return _osKeyPress; }
            set { _osKeyPress = value; }
        }


        public string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";
            return "TPMPanelSwitch{" + onStr + Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch) + "}" + SeparatorChars + _osKeyPress.ExportString();
        }

        public bool WhenTurnedOn
        {
            get { return _whenOnTurnedOn; }
            set { _whenOnTurnedOn = value; }
        }




    }
}
