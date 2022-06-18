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
                
                if (keySequence == null)
                    return;

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

        private string GetEventString<Type>(int switchId, bool pressed)
        {
            return $" {Enum.GetName(typeof(Type), switchId)}  {(pressed == false ? "RELEASED" : "PRESSED")}  ";
        }

        private string GetInfoFromEnums(string profile, string panelHidId, int panelId, int switchId, bool pressed)
        {
            var panel = (PluginGamingPanelEnum)panelId; // Enum.GetName(typeof(GamingPanelInternalEnum), panelId);
            var result = $"{DateTime.Now:dd.MM.yyyy hh:mm:ss} {profile} {Enum.GetName(typeof(PluginGamingPanelEnum), panelId)}  ";

            result += panel switch
            {
                PluginGamingPanelEnum.PZ55SwitchPanel => GetEventString<SwitchPanelPZ55Keys>(switchId, pressed),
                PluginGamingPanelEnum.PZ69RadioPanel => GetEventString<RadioPanelPZ69KnobsEmulator>(switchId, pressed),
                PluginGamingPanelEnum.PZ70MultiPanel => GetEventString<MultiPanelPZ70Knobs>(switchId, pressed),
                PluginGamingPanelEnum.TPM => GetEventString<TPMPanelSwitches>(switchId, pressed),
                PluginGamingPanelEnum.FarmingPanel => GetEventString<FarmingPanelMKKeys>(switchId, pressed),
                
                PluginGamingPanelEnum.StreamDeckMK2
                or PluginGamingPanelEnum.StreamDeckV2
                or PluginGamingPanelEnum.StreamDeck
                or PluginGamingPanelEnum.StreamDeckMini
                or PluginGamingPanelEnum.StreamDeckXL => GetEventString<EnumStreamDeckButtonNames>(switchId, pressed),
                
                PluginGamingPanelEnum.BackLitPanel => "BIP Event - ATTN : this panel cannot produce events",
                
                PluginGamingPanelEnum.PZ69RadioPanel_PreProg_MI24P => GetEventString<RadioPanelPZ69KnobsMi24P>(switchId, pressed),
                
                PluginGamingPanelEnum.Unknown 
                or _ => "Unknown panel."
            };
            return result += $" {panelHidId}\n";
        }
    }
}
