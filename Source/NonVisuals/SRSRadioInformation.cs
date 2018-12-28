namespace NonVisuals
{
    // ReSharper disable All
    /*
     * Do not change SRS code
     */
    public class SRSRadioInformation
    {
        public enum EncryptionMode
        {
            NO_ENCRYPTION = 0,
            ENCRYPTION_JUST_OVERLAY = 1,
            ENCRYPTION_FULL = 2,
            ENCRYPTION_COCKPIT_TOGGLE_OVERLAY_CODE = 3

            // 0  is no controls
            // 1 is FC3 Gui Toggle + Gui Enc key setting
            // 2 is InCockpit toggle + Incockpit Enc setting
            // 3 is Incockpit toggle + Gui Enc Key setting
        }

        public enum VolumeMode
        {
            COCKPIT = 0,
            OVERLAY = 1,
        }

        public enum FreqMode
        {
            COCKPIT = 0,
            OVERLAY = 1,
        }

        public enum Modulation
        {
            AM = 0,
            FM = 1,
            INTERCOM = 2,
            DISABLED = 3
        }

        public bool enc = false; // encrytion enabled
        public byte encKey = 0;
        public EncryptionMode encMode = EncryptionMode.NO_ENCRYPTION;

        public double freqMax = 1;
        public double freqMin = 1;
        public double freq = 1;
        public Modulation modulation = Modulation.DISABLED;
        public string name = "";
        public double secFreq = 1;
        public float volume = 1.0f;

        public FreqMode freqMode = FreqMode.COCKPIT;
        public VolumeMode volMode = VolumeMode.COCKPIT;

        public bool expansion = false;

        public int channel = -1;

        /**
         * Used to determine if we should send an update to the server or not
         * We only need to do that if something that would stop us Receiving happens which
         * is frequencies and modulation
         */

        public override bool Equals(object obj)
        {
            if ((obj == null) || (GetType() != obj.GetType()))
                return false;

            var compare = (SRSRadioInformation)obj;

            if (!name.Equals(compare.name))
            {
                return false;
            }
            if (freq != compare.freq)
            {
                return false;
            }

            if (modulation != compare.modulation)
            {
                return false;
            }
            if (secFreq != compare.secFreq)
            {
                return false;
            }
            //if (volume != compare.volume)
            //{
            //    return false;
            //}
            //if (freqMin != compare.freqMin)
            //{
            //    return false;
            //}
            //if (freqMax != compare.freqMax)
            //{
            //    return false;
            //}


            return true;
        }

    }
}
