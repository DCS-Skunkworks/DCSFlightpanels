namespace NonVisuals.Panels.StreamDeck
{
    using System;
    using System.Windows.Controls;

    using ClassLibraryCommon;

    using MEF;

    public static class StreamDeckCommon
    {
        public static PluginGamingPanelEnum ConvertEnum(GamingPanelEnum gamingPanel)
        {
            return gamingPanel switch
            {
                GamingPanelEnum.StreamDeckMini => PluginGamingPanelEnum.StreamDeckMini,
                GamingPanelEnum.StreamDeck => PluginGamingPanelEnum.StreamDeck,
                GamingPanelEnum.StreamDeckV2 => PluginGamingPanelEnum.StreamDeckV2,
                GamingPanelEnum.StreamDeckMK2 => PluginGamingPanelEnum.StreamDeckMK2,
                GamingPanelEnum.StreamDeckXL => PluginGamingPanelEnum.StreamDeckXL,
                _ => PluginGamingPanelEnum.Unknown
            };
        }

        public static int ButtonNumber(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            return streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON ?
            -1 :
            int.Parse(streamDeckButtonName.ToString().Replace("BUTTON", string.Empty));
        }

        public static EnumStreamDeckButtonNames ButtonName(int streamDeckButtonNumber)
        {
            return streamDeckButtonNumber == 0 ?
            EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON :
            (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), $"BUTTON{streamDeckButtonNumber}");
        }

        public static EnumStreamDeckButtonNames ButtonName(string streamDeckButtonNumber)
        {
            if (string.IsNullOrEmpty(streamDeckButtonNumber) || streamDeckButtonNumber == "0")
            {
                return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }

        public static EnumComparator GetComparatorValue(string text)
        {
            return text switch
            {
                "NotSet" => EnumComparator.NotSet,
                "==" => EnumComparator.Equals,
                "!=" => EnumComparator.NotEquals,
                "<" => EnumComparator.LessThan,
                "<=" => EnumComparator.LessThanEqual,
                ">" => EnumComparator.GreaterThan,
                ">=" => EnumComparator.GreaterThanEqual,
                "Always" => EnumComparator.Always,
                _ => throw new Exception($"Failed to decode comparison type [{text}]")
            };
        }

        public static void SetComparatorValue(ComboBox comboBox, EnumComparator comparator)
        {
            comboBox.Text = comparator switch
            {
                EnumComparator.NotSet => "NotSet",
                EnumComparator.Equals => "==",
                EnumComparator.NotEquals => "!=",
                EnumComparator.LessThan => "<",
                EnumComparator.LessThanEqual => "<=",
                EnumComparator.GreaterThan => ">",
                EnumComparator.GreaterThanEqual => ">=",
                EnumComparator.Always => "Always",
                _ => throw new Exception($"Failed to decode comparison type [{comparator}]")
            };
        }
    }
}
