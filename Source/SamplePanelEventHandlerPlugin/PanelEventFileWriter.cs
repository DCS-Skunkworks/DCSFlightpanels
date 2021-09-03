namespace SamplePanelEventPlugin
{
    using System;
    using System.IO;

    public class PanelEventFileWriter
    {
        private static PanelEventFileWriter _panelEventFileWriter;
        private readonly string _filePath;
        private readonly object _lockObject = new object();
        private string _filename = "DCSFP_Plugin_Output.txt";

        public PanelEventFileWriter()
        {
            _filePath = AppDomain.CurrentDomain.BaseDirectory;
        }


        public static void WriteInfo(string profile, string panelHidId, int panelId, int switchId, int eventType, int extraInfo)
        {
            if (_panelEventFileWriter == null)
            {
                _panelEventFileWriter = new PanelEventFileWriter();
            }
            _panelEventFileWriter.WriteInfoToFile(profile, panelHidId, panelId, switchId, eventType, extraInfo);
        }

        private void WriteInfoToFile(string profile, string panelHidId, int panelId, int switchId, int eventType, int extraInfo)
        {
            lock (_lockObject)
            {
                File.AppendAllText(_filePath + _filename, GetInfoFromEnums(profile, panelHidId, panelId, switchId, eventType, extraInfo));
            }
        }

        private string GetInfoFromEnums(string profile, string panelHidId, int panelId, int switchId, int eventType, int extraInfo)
        {
            var panel = (PluginGamingPanelEnum)panelId;//Enum.GetName(typeof(GamingPanelInternalEnum), panelId);
            var result = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " " + profile + " " + Enum.GetName(typeof(PluginGamingPanelEnum), panelId) + "  ";

            switch (panel)
            {
                case PluginGamingPanelEnum.Unknown:
                    {
                        return "Unknown panel. " + panelHidId;
                    }
                case PluginGamingPanelEnum.PZ55SwitchPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(SwitchPanelPZ55Keys), switchId) + "  " + (eventType == 0 ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.PZ69RadioPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), switchId) + "  " + (eventType == 0 ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.PZ70MultiPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(MultiPanelPZ70Knobs), switchId) + "  " + (eventType == 0 ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.BackLitPanel:
                    {
                        return "BIP Event - ATTN : this panel cannot produce events";
                    }
                case PluginGamingPanelEnum.TPM:
                    {
                        result = result + " " + Enum.GetName(typeof(TPMPanelSwitches), switchId) + "  " + (eventType == 0 ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.StreamDeckMK2:
                case PluginGamingPanelEnum.StreamDeckV2:
                case PluginGamingPanelEnum.StreamDeck:
                case PluginGamingPanelEnum.StreamDeckMini:
                case PluginGamingPanelEnum.StreamDeckXL:
                    {
                        result = result + " " + Enum.GetName(typeof(EnumStreamDeckButtonNames), switchId) + "  " + (eventType == 0 ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
                case PluginGamingPanelEnum.FarmingPanel:
                    {
                        result = result + " " + Enum.GetName(typeof(FarmingPanelMKKeys), switchId) + "  " + (eventType == 0 ? "RELEASED" : "PRESSED") + "  ";
                        break;
                    }
            }

            result = result + " " + panelHidId + "\n";
            return result;
        }
    }
}
