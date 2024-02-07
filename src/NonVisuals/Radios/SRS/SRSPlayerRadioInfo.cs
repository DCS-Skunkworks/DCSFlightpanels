using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NonVisuals.Radios.SRS
{
    public class SRSPlayerRadioInfo
    {
        //HOTAS or IN COCKPIT controls
        public enum RadioSwitchControls
        {
            HOTAS = 0,
            IN_COCKPIT = 1
        }

        
        
        public string name = "";

        
        
        public SRSLatLngPosition latLng = new();

        
        
        public bool inAircraft = false;

        
        
        public volatile bool ptt = false;

        public SRSRadioInformation[] radios = new SRSRadioInformation[11]; //10 + intercom

        
        
        public RadioSwitchControls control = RadioSwitchControls.HOTAS;

        
        public short selected = 0;

        public string unit = "";

        public uint unitId;

        
        
        public int seat = 0;

        
        
        public bool intercomHotMic = false; //if true switch to intercom and transmit

        public SRSTransponder iff = new();

        [JsonIgnore]
        public readonly static uint UnitIdOffset = 100000000
            ; // this is where non aircraft "Unit" Ids start from for satcom intercom

        
        
        public bool simultaneousTransmission = false; // Global toggle enabling simultaneous transmission on multiple radios, activated via the AWACS panel

        
        public SimultaneousTransmissionControl simultaneousTransmissionControl =
            SimultaneousTransmissionControl.EXTERNAL_DCS_CONTROL;

        
        public SRSAircraftCapabilities capabilities = new();

        public enum SimultaneousTransmissionControl
        {
            ENABLED_INTERNAL_SRS_CONTROLS = 1,
            EXTERNAL_DCS_CONTROL = 0,
        }

        public SRSPlayerRadioInfo()
        {
            for (var i = 0; i < 11; i++)
            {
                radios[i] = new SRSRadioInformation();
            }
        }

        [JsonIgnore]
        public long LastUpdate { get; set; }

        public void Reset()
        {
            name = "";
            latLng = new SRSLatLngPosition();
            ptt = false;
            selected = 0;
            unit = "";
            simultaneousTransmission = false;
            simultaneousTransmissionControl = SimultaneousTransmissionControl.EXTERNAL_DCS_CONTROL;
            LastUpdate = 0;
            seat = 0;

            for (var i = 0; i < 11; i++)
            {
                radios[i] = new SRSRadioInformation();
            }

        }

        // override object.Equals
        public override bool Equals(object compare)
        {
            try
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

                if (inAircraft != compareRadio.inAircraft)
                {
                    return false;
                }

                if (((iff == null) || (compareRadio.iff == null)))
                {
                    return false;
                }
                else
                {
                    //check iff
                    if (!iff.Equals(compareRadio.iff))
                    {
                        return false;
                    }
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
            }
            catch
            {
                return false;
            }


            return true;
        }


        /*
         * Was Radio updated in the last 10 Seconds
         */

        public bool IsCurrent()
        {
            return LastUpdate > DateTime.Now.Ticks - 100000000;
        }

        //comparing doubles is risky - check that we're close enough to hear (within 100hz)
        public static bool FreqCloseEnough(double freq1, double freq2)
        {
            var diff = Math.Abs(freq1 - freq2);

            return diff < 500;
        }

        public SRSRadioInformation CanHearTransmission(double frequency,
            SRSRadioInformation.Modulation modulation,
            byte encryptionKey,
            bool strictEncryption,
            uint sendingUnitId,
            List<int> blockedRadios,
            out SRSRadioReceivingState receivingState,
            out bool decryptable)
        {
            //    if (!IsCurrent())
            //     {
            //         receivingState = null;
            //        decryptable = false;
            //       return null;
            //   }

            SRSRadioInformation bestMatchingRadio = null;
            SRSRadioReceivingState bestMatchingRadioState = null;
            bool bestMatchingDecryptable = false;

            for (var i = 0; i < radios.Length; i++)
            {
                var receivingRadio = radios[i];

                if (receivingRadio != null)
                {
                    //handle INTERCOM Modulation is 2
                    if ((receivingRadio.modulation == SRSRadioInformation.Modulation.INTERCOM) &&
                        (modulation == SRSRadioInformation.Modulation.INTERCOM))
                    {
                        if ((unitId > 0) && (sendingUnitId > 0)
                            && (unitId == sendingUnitId))
                        {
                            receivingState = new SRSRadioReceivingState
                            {
                                IsSecondary = false,
                                LastReceivedAt = DateTime.Now.Ticks,
                                ReceivedOn = i
                            };
                            decryptable = true;
                            return receivingRadio;
                        }
                        decryptable = false;
                        receivingState = null;
                        return null;
                    }

                    if (modulation == SRSRadioInformation.Modulation.DISABLED
                        || receivingRadio.modulation == SRSRadioInformation.Modulation.DISABLED)
                    {
                        continue;
                    }

                    //within 1khz
                    if ((FreqCloseEnough(receivingRadio.freq, frequency))
                        && (receivingRadio.modulation == modulation)
                        && (receivingRadio.freq > 10000))
                    {
                        bool isDecryptable = (receivingRadio.enc ? receivingRadio.encKey : (byte)0) == encryptionKey || (!strictEncryption && encryptionKey == 0);

                        if (isDecryptable && !blockedRadios.Contains(i))
                        {
                            receivingState = new SRSRadioReceivingState
                            {
                                IsSecondary = false,
                                LastReceivedAt = DateTime.Now.Ticks,
                                ReceivedOn = i
                            };
                            decryptable = true;
                            return receivingRadio;
                        }

                        bestMatchingRadio = receivingRadio;
                        bestMatchingRadioState = new SRSRadioReceivingState
                        {
                            IsSecondary = false,
                            LastReceivedAt = DateTime.Now.Ticks,
                            ReceivedOn = i
                        };
                        bestMatchingDecryptable = isDecryptable;
                    }
                    if ((receivingRadio.secFreq == frequency)
                        && (receivingRadio.secFreq > 10000))
                    {
                        if ((receivingRadio.enc ? receivingRadio.encKey : (byte)0) == encryptionKey || (!strictEncryption && encryptionKey == 0))
                        {
                            receivingState = new SRSRadioReceivingState
                            {
                                IsSecondary = true,
                                LastReceivedAt = DateTime.Now.Ticks,
                                ReceivedOn = i
                            };
                            decryptable = true;
                            return receivingRadio;
                        }

                        bestMatchingRadio = receivingRadio;
                        bestMatchingRadioState = new SRSRadioReceivingState
                        {
                            IsSecondary = true,
                            LastReceivedAt = DateTime.Now.Ticks,
                            ReceivedOn = i
                        };
                    }
                }
            }

            decryptable = bestMatchingDecryptable;
            receivingState = bestMatchingRadioState;
            return bestMatchingRadio;
        }

        public SRSPlayerRadioInfo DeepClone()
        {
            var clone = (SRSPlayerRadioInfo)MemberwiseClone();

            clone.iff = iff.Copy();
            //ignore position
            clone.radios = new SRSRadioInformation[11];

            for (var i = 0; i < 11; i++)
            {
                clone.radios[i] = radios[i].Copy();
            }

            return clone;

        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                foreach (var radio in radios)
                {
                    result = (result * 397) ^ radio.GetHashCode();
                }
                result = (result * 397) ^ name.GetHashCode();
                result = (result * 397) ^ latLng.GetHashCode();
                result = (result * 397) ^ inAircraft.GetHashCode();
                result = (result * 397) ^ ptt.GetHashCode();
                result = (result * 397) ^ control.GetHashCode();
                result = (result * 397) ^ selected.GetHashCode();
                result = (result * 397) ^ unit.GetHashCode();
                result = (result * 397) ^ unitId.GetHashCode();
                result = (result * 397) ^ seat.GetHashCode();
                result = (result * 397) ^ intercomHotMic.GetHashCode();
                result = (result * 397) ^ iff.GetHashCode();
                return result;
            }
        }
    }
}
