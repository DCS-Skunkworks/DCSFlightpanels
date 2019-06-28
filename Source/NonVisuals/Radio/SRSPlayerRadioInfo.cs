using System;
using Newtonsoft.Json;
// ReSharper disable All
/*
 * Do not change SRS code
 */
namespace NonVisuals.Radio
{


    public class SRSRadioReceivingState
    {
        [JsonIgnore]
        public double LastReceviedAt { get; set; }

        public bool IsSecondary { get; set; }
        public int ReceivedOn { get; set; }

        public bool PlayedEndOfTransmission { get; set; }

        public bool IsReceiving => (Environment.TickCount - LastReceviedAt) < 500;
    }

    public class SRSRadioSendingState
    {
        [JsonIgnore]
        public double LastSentAt { get; set; }

        public bool IsSending { get; set; }

        public int SendingOn { get; set; }

        public int IsEncrypted { get; set; }
    }

    public struct SRSCombinedRadioState
    {
        public SRSPlayerRadioInfo RadioInfo;

        public SRSRadioSendingState RadioSendingState;

        public SRSRadioReceivingState[] RadioReceivingState;
    }

    public struct SRSDcsPosition
    {
        public double x;
        public double y;
        public double z;

        public override string ToString()
        {
            return $"Pos:[{x},{y},{z}]";
        }
    }

    public class SRSPlayerRadioInfo
    {

        //HOTAS or IN COCKPIT controls
        public enum RadioSwitchControls
        {
            HOTAS = 0,
            IN_COCKPIT = 1
        }

        public string name = "";
        public SRSDcsPosition pos = new SRSDcsPosition();
        public volatile bool ptt = false;

        public SRSRadioInformation[] radios = new SRSRadioInformation[11]; //10 + intercom
        public RadioSwitchControls control = RadioSwitchControls.HOTAS;
        public short selected = 0;
        public string unit = "";
        public uint unitId;

        public readonly static uint UnitIdOffset = 100000001; // this is where non aircraft "Unit" Ids start from for satcom intercom

        public SRSPlayerRadioInfo()
        {
            for (var i = 0; i < 11; i++)
            {
                radios[i] = new SRSRadioInformation();
            }
        }

        [JsonIgnore]
        public long LastUpdate { get; set; }

        // override object.Equals
        public override bool Equals(object compare)
        {
            if ((compare == null) || (GetType() != compare.GetType()))
            {
                return false;
            }

            var compareRadio = compare as SRSPlayerRadioInfo;

            if (control != compareRadio.control)
            {
                return false;
            }
            //if (side != compareRadio.side)
            //{
            //    return false;
            //}
            if (!name.Equals(compareRadio.name))
            {
                return false;
            }
            if (!unit.Equals(compareRadio.unit))
            {
                return false;
            }

            if (unitId != compareRadio.unitId)
            {
                return false;
            }

            for (var i = 0; i < radios.Length; i++)
            {
                var radio1 = radios[i];
                var radio2 = compareRadio.radios[i];

                if ((radio1 != null) && (radio2 != null))
                {
                    if (!radio1.Equals(radio2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        /*
         * Was Radio updated in the last 10 Seconds
         */

        public bool IsCurrent()
        {
            return LastUpdate > Environment.TickCount - 10000;
        }

    }
}
