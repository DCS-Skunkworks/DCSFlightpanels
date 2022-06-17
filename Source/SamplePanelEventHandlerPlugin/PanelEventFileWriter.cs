namespace SamplePanelEventPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using MEF;
    using NonVisuals.Radios.Knobs;

    public class PanelEventFileWriter
    {
        private static PanelEventFileWriter _panelEventFileWriter;
        private readonly string _filePath;
        private readonly object _lockObject = new object();
        private readonly string _filename = "DCSFP_Plugin_Output.txt";

        public PanelEventFileWriter()
        {
            _filePath = AppDomain.CurrentDomain.BaseDirectory;
        }


        public static void WriteInfo(string profile, string panelHidId, int panelId, int switchId, bool pressed, SortedList<int, IKeyPressInfo> keySequence)
        {
            if (_panelEventFileWriter == null)
            {
                _panelEventFileWriter = new PanelEventFileWriter();
            }
            _panelEventFileWriter.WriteInfoToFile(profile, panelHidId, panelId, switchId, pressed, keySequence);
        }

        private void WriteInfoToFile(string profile, string panelHidId, int panelId, int switchId, bool pressed, SortedList<int, IKeyPressInfo> keySequence)
        {
            lock (_lockObject)
            {
                File.AppendAllText(_filePath + _filename, GetInfoFromEnums(profile, panelHidId, panelId, switchId, pressed));

                if (keySequence != null)
                {
                    foreach (var keyPressInfo in keySequence)
                    {
                        File.AppendAllText(
                            _filePath + _filename,
                            "\t" + 
                            Enum.GetName(typeof(KeyPressLength), keyPressInfo.Value.LengthOfBreak) + " " +
                            Enum.GetName(typeof(KeyPressLength), keyPressInfo.Value.LengthOfKeyPress) + " " +
                            keyPressInfo.Value.VirtualKeyCodesAsString
                            + "\n");
                    }
                }
            }
        }

        private string GetInfoFromEnums(string profile, string panelHidId, int panelId, int switchId, bool pressed)
        {
            var panel = (PluginGamingPanelEnum)panelId; // Enum.GetName(typeof(GamingPanelInternalEnum), panelId);
            var result = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " " + profile + " " + Enum.GetName(typeof(PluginGamingPanelEnum), panelId) + "  ";

            switch (panel)
            {
                case PluginGamingPanelEnum.Unknown:
                    {
                        return "Unknown panel. " + panelHidId;
                    }
                case PluginGamingPanelEnum.PZ55SwitchPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(SwitchPanelPZ55Keys), switchId) + "  " + (pressed == false ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.PZ69RadioPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), switchId) + "  " + (pressed == false ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.PZ70MultiPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(MultiPanelPZ70Knobs), switchId) + "  " + (pressed == false ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.BackLitPanel:
                    {
                        return "BIP Event - ATTN : this panel cannot produce events";
                    }
                case PluginGamingPanelEnum.TPM:
                    {
                        result = result + " " + Enum.GetName(typeof(TPMPanelSwitches), switchId) + "  " + (pressed == false ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.StreamDeckMK2:
                case PluginGamingPanelEnum.StreamDeckV2:
                case PluginGamingPanelEnum.StreamDeck:
                case PluginGamingPanelEnum.StreamDeckMini:
                case PluginGamingPanelEnum.StreamDeckXL:
                    {
                        result = result + " " + Enum.GetName(typeof(EnumStreamDeckButtonNames), switchId) + "  " + (pressed == false ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.FarmingPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(FarmingPanelMKKeys), switchId) + "  " + (pressed == false ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.PZ69RadioPanel_MI24P:
                    {
                        result = result + " " + Enum.GetName(typeof(RadioPanelPZ69KnobsMi24P), switchId) + "  " + (pressed == false ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
            }

            result = result + " " + panelHidId + "\n";
            return result;
        }
    }
}
