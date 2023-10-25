using System;
using System.Windows.Diagnostics;

namespace NonVisuals.Radios.RadioControls
{
    public enum ARC210FrequencyBand
    {
        FM,
        VHF1,
        VHF2,
        UHF
    }

    internal class ARC210
    {
        /*ARC210*/
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        private uint _arc210BigFrequencyStandby = 108;
        private uint _arc210SmallFrequencyStandby;
        private readonly uint[] _lowerFrequencyBounds = { 30, 108, 118, 225 };
        private readonly uint[] _higherFrequencyBounds = { 87, 115, 173, 399 };
        private readonly uint[] _lastBigFrequencyPerBand = { 30, 108, 118, 225 };
        private readonly uint[] _lastSmallFrequencyPerBand = { 0, 0, 0, 0 };
        private const int FM = 0;
        private const int FM_MIN = 30;
        private const int FM_MAX = 87;
        private const int VHF1 = 1;
        private const int VHF1_MIN = 108;
        private const int VHF1_MAX = 115;
        private const int VHF2 = 2;
        private const int VHF2_MIN = 118;
        private const int VHF2_MAX = 173;
        private const int UHF = 3;
        private const int UHF_MIN = 225;
        private const int UHF_MAX = 399;
        private readonly uint _higherChangeRate;
        private const uint QUART_FREQ_CHANGE_VALUE = 25;
        private const uint BIG_FREQ_CHANGE_VALUE = 1;
        private ARC210FrequencyBand _currentARC210FrequencyBand;
        private ARC210FrequencyBand _newARC210FrequencyBand;
        private readonly string _dcsbiosIdentifier;
        private string _cockpitFrequency = "108.000";
        private readonly int _frequencyBandSkipCount; // must skip, otherwise it is difficult getting correct band
        private int _frequencyBandSkipCounter;
        private ARC210FrequencyBand[] _supportedFrequencyBands;

        public ARC210(string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, uint higherChangeRate, int frequencyBandSkipCount, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            _dcsbiosIdentifier = dcsbiosIdentifier.Trim();
            _higherChangeRate = higherChangeRate;
            _frequencyBandSkipCount = frequencyBandSkipCount;
            _supportedFrequencyBands = supportedFrequencyBands;
            SetFrequencyBand(initialFrequencyBand);
            SetDefaultStandbyFromFrequencyBand(initialFrequencyBand);

            if(supportedFrequencyBands.i)
        }

        /* (FM) 30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        public void BigFrequencyUp(bool changeFaster = false)
        {
            switch (_currentARC210FrequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        if (GetBigFrequencyStandby() >= FM_MAX || (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= FM_MAX))
                        {
                            SetBigFrequencyStandby(FM_MIN);
                            break;
                        }

                        AddBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        if (GetBigFrequencyStandby() >= VHF1_MAX || (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= VHF1_MAX))
                        {
                            SetBigFrequencyStandby(VHF1_MIN);
                            break;
                        }

                        AddBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        if (GetBigFrequencyStandby() >= VHF2_MAX || (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= VHF2_MAX))
                        {
                            SetBigFrequencyStandby(VHF2_MIN);
                            break;
                        }

                        AddBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        if (GetBigFrequencyStandby() >= UHF_MAX || (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= UHF_MAX))
                        {
                            SetBigFrequencyStandby(UHF_MIN);
                            break;
                        }

                        AddBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
            }
        }

        public void BigFrequencyDown(bool changeFaster = false)
        {
            switch (_currentARC210FrequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        if (GetBigFrequencyStandby() <= FM_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= FM_MIN))
                        {
                            SetBigFrequencyStandby(FM_MIN);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        if (GetBigFrequencyStandby() <= VHF1_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= VHF1_MIN))
                        {
                            SetBigFrequencyStandby(VHF1_MIN);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        if (GetBigFrequencyStandby() <= VHF2_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= VHF2_MIN))
                        {
                            SetBigFrequencyStandby(VHF2_MIN);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        if (GetBigFrequencyStandby() <= UHF_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= UHF_MIN))
                        {
                            SetBigFrequencyStandby(UHF_MIN);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
            }
        }

        public void SmallFrequencyUp()
        {
            if (GetSmallFrequencyStandby() >= 975)
            {
                SetSmallFrequencyStandby(0);
                return;
            }

            AddSmallFrequencyStandby(QUART_FREQ_CHANGE_VALUE);
        }

        public void SmallFrequencyDown()
        {
            if (GetSmallFrequencyStandby() == 0)
            {
                SetSmallFrequencyStandby(975);
                return;
            }
            SubtractSmallFrequencyStandby(QUART_FREQ_CHANGE_VALUE);
        }

        public string GetStandbyFrequency()
        {
            return GetBigFrequencyStandby() + "." + GetSmallFrequencyStandby().ToString().PadLeft(3, '0').Trim();
        }

        public string GetCockpitFrequency()
        {
            return _cockpitFrequency.Trim();
        }

        public string GetFrequencyBandId()
        {
            return ((int)_currentARC210FrequencyBand).ToString();
        }

        public string GetTemporaryFrequencyBandId()
        {
            return ((int)_newARC210FrequencyBand).ToString();
        }

        public void SetCockpitFrequency(string frequency)
        {
            /*
             * A new frequency has been received from DCS-BIOS.
             *
             * a) user has pressed the button to set the frequency
             * b) user has changed the frequency in the cockpit
             */
            

            SetStandbyFrequency(_cockpitFrequency);
            _cockpitFrequency = frequency;

            var newBand = GetFrequencyBand(_cockpitFrequency);
            var oldBand = GetFrequencyBand(GetStandbyFrequency());
            if (SwitchFrequencyBands(newBand, oldBand))
            {
                _currentARC210FrequencyBand = newBand;
                _newARC210FrequencyBand = newBand;
            }
        }

        private void SetStandbyFrequency(string frequency)
        {
            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            SetBigFrequencyStandby(uint.Parse(array[0]));
            SetSmallFrequencyStandby(uint.Parse(array[1]));
        }

        public string GetDCSBIOSCommand()
        {
            return $"{_dcsbiosIdentifier} {GetStandbyFrequency()}\n";
        }

        private ARC210FrequencyBand GetFrequencyBand(string frequency)
        {
            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var bigFrequencyStandby = uint.Parse(array[0]);

            if (bigFrequencyStandby >= _lowerFrequencyBounds[FM] && bigFrequencyStandby <= _higherFrequencyBounds[FM])
            {
                return ARC210FrequencyBand.FM;
            }
            if (bigFrequencyStandby >= _lowerFrequencyBounds[VHF1] && bigFrequencyStandby <= _higherFrequencyBounds[VHF1])
            {
                return ARC210FrequencyBand.VHF1;
            }
            if (bigFrequencyStandby >= _lowerFrequencyBounds[VHF2] && bigFrequencyStandby <= _higherFrequencyBounds[VHF2])
            {
                return ARC210FrequencyBand.VHF2;
            }
            if (bigFrequencyStandby >= _lowerFrequencyBounds[UHF] && bigFrequencyStandby <= _higherFrequencyBounds[UHF])
            {
                return ARC210FrequencyBand.UHF;
            }

            throw new Exception("ARC210 : Frequency not matching any frequency bands.");
        }

        private void SetFrequencyBand(ARC210FrequencyBand arc210FrequencyBand)
        {
            _currentARC210FrequencyBand = arc210FrequencyBand;
        }

        public void TemporaryFrequencyBandUp()
        {
            _frequencyBandSkipCounter++;
            if (_frequencyBandSkipCounter < _frequencyBandSkipCount)
            {
                return;
            }

            _frequencyBandSkipCounter = 0;

            switch (_newARC210FrequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.VHF1;
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.VHF2;
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.UHF;
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.FM;
                        break;
                    }
            }
        }

        public void TemporaryFrequencyBandDown()
        {
            _frequencyBandSkipCounter++;
            if (_frequencyBandSkipCounter < _frequencyBandSkipCount)
            {
                return;
            }

            _frequencyBandSkipCounter = 0;

            switch (_newARC210FrequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.UHF;
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.FM;
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.VHF1;
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        _newARC210FrequencyBand = ARC210FrequencyBand.VHF2;
                        break;
                    }
            }
        }

        public void SwitchFrequencyBand()
        {
            SwitchFrequencyBands(_newARC210FrequencyBand, _currentARC210FrequencyBand);
            _currentARC210FrequencyBand = _newARC210FrequencyBand;
        }

        private bool SwitchFrequencyBands(ARC210FrequencyBand newFrequencyBandBand, ARC210FrequencyBand oldFrequencyBandBand)
        {
            if (newFrequencyBandBand == oldFrequencyBandBand)
            {
                return false;
            }

            switch (oldFrequencyBandBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        _lastBigFrequencyPerBand[FM] = GetBigFrequencyStandby();
                        _lastSmallFrequencyPerBand[FM] = GetSmallFrequencyStandby();
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        _lastBigFrequencyPerBand[VHF1] = GetBigFrequencyStandby();
                        _lastSmallFrequencyPerBand[VHF1] = GetSmallFrequencyStandby();
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        _lastBigFrequencyPerBand[VHF2] = GetBigFrequencyStandby();
                        _lastSmallFrequencyPerBand[VHF2] = GetSmallFrequencyStandby();
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        _lastBigFrequencyPerBand[UHF] = GetBigFrequencyStandby();
                        _lastSmallFrequencyPerBand[UHF] = GetSmallFrequencyStandby();
                        break;
                    }
            }

            switch (newFrequencyBandBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[FM]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[FM]);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[VHF1]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[VHF1]);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[VHF2]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[VHF2]);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[UHF]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[UHF]);
                        break;
                    }
            }

            return true;
        }

        private bool SetDefaultStandbyFromFrequencyBand(ARC210FrequencyBand frequencyBand)
        {
            switch (frequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[FM]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[FM]);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[VHF1]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[VHF1]);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[VHF2]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[VHF2]);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        SetBigFrequencyStandby(_lastBigFrequencyPerBand[UHF]);
                        SetSmallFrequencyStandby(_lastSmallFrequencyPerBand[UHF]);
                        break;
                    }
            }

            return true;
        }

        private uint GetBigFrequencyCockpit()
        {
            return uint.Parse(_cockpitFrequency.Split('.',StringSplitOptions.RemoveEmptyEntries)[0]);
        }
        
        private uint GetSmallFrequencyCockpit()
        {
            return uint.Parse(_cockpitFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        private void SetBigFrequencyStandby(uint value)
        {
            _arc210BigFrequencyStandby = value;
        }

        private void AddBigFrequencyStandby(uint value)
        {
            _arc210BigFrequencyStandby += value;
        }

        private void SubtractBigFrequencyStandby(uint value)
        {
            _arc210BigFrequencyStandby -= value;
        }

        private uint GetBigFrequencyStandby()
        {
            return _arc210BigFrequencyStandby;
        }

        private void SetSmallFrequencyStandby(uint value)
        {
            _arc210SmallFrequencyStandby = value;
        }

        private void AddSmallFrequencyStandby(uint value)
        {
            _arc210SmallFrequencyStandby += value;
        }

        private void SubtractSmallFrequencyStandby(uint value)
        {
            _arc210SmallFrequencyStandby -= value;
        }

        private uint GetSmallFrequencyStandby()
        {
            return _arc210SmallFrequencyStandby;
        }

        private bool IsFrequencyBandSupported(ARC210FrequencyBand frequencyBand)
        {
            foreach (var supportedBand in _supportedFrequencyBands)
            {
                if (frequencyBand == supportedBand) return true;
            }

            return false;
        }
    }
}
