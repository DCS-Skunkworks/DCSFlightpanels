﻿using System;
using System.Text;

namespace NonVisuals
{

    public enum BIPLightDelays :int
    {
        Zeroms = 0,
        Fiftyms = 50,
        Hundredms = 100,
        TwoHundredms = 200,
        ThreeHundredms = 300,
        ForHundredms = 400,
        FiveHundredms = 500,
        OneSec = 1000,
        OneAndHalfSec = 1500,
        TwoSec = 2000,
        ThreeSec = 3000,
        FourSec = 4000,
        FiveSec = 5000,
        SixSec = 6000,
        SevenSec = 7000,
        EightSec = 8000,
        NineSec = 9000,
        TenSec = 10000,
    }

    public abstract class PanelSwitchBIPLightBinding
    {
        public abstract void ImportSettings(string settings);
        public abstract string ExportSettings();

        public PanelLEDColor LEDColor { get; set; }
        public BIPLedPositionEnum BIPLedPosition { get; set; }
        public BIPLightDelays DelayBefore { get; set; }
        public string Hash { get; set; }
        public string[] Separator { get; } = { "|" };
    }

    public class BIPLight : PanelSwitchBIPLightBinding
    {
        public override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                return;
            }
            //BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}
            settings = settings.Replace("BIPLight{", "");
            settings = settings.Replace("}", "");
            //Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e
            var settingsArray = settings.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            //Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e
            BIPLedPosition = (BIPLedPositionEnum)Enum.Parse(typeof(BIPLedPositionEnum), settingsArray[0].ToString());
            LEDColor = (PanelLEDColor)Enum.Parse(typeof(PanelLEDColor), settingsArray[1].ToString());
            DelayBefore = (BIPLightDelays) Enum.Parse(typeof(BIPLightDelays), settingsArray[2].ToString());
            Hash = settingsArray[3].ToString();
        }

        public override string ExportSettings()
        {
            //BIPLight{Position_1_4|GREEN|FourSec|f5fe6e63e0c05a20f519d4b9e46fab3e}
            var stringBuilder = new StringBuilder();
            var position = BIPLedPosition.ToString();
            stringBuilder.Append("BIPLight{" + position + "|" + LEDColor + "|" + DelayBefore + "|" + Hash + "}");
            return stringBuilder.ToString();
        }
    }
}
