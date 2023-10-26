using System;
using System.Linq;
using System.Windows.Diagnostics;

namespace NonVisuals.Radios.RadioControls
{
    internal enum ARC210FrequencyBand
    {
        FM,
        VHF1,
        VHF2,
        UHF
    }

    internal class ARC210
    {
        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        private uint _arc210BigFrequencyStandby = 108;
        private uint _arc210SmallFrequencyStandby;
        private readonly uint[] _lowerFrequencyBounds = { 30, 108, 118, 225 };
        private readonly uint[] _higherFrequencyBounds = { 87, 115, 173, 399 };
        private readonly uint[] _savedBigFrequencyPerBand = { 30, 108, 118, 225 };
        private readonly uint[] _savedSmallFrequencyPerBand = { 0, 0, 0, 0 };
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
        private readonly ARC210FrequencyBand _initialFrequencyBand;
        private readonly uint _higherChangeRate;
        private const uint QUART_FREQ_CHANGE_VALUE = 25;
        private const uint BIG_FREQ_CHANGE_VALUE = 1;
        private ARC210FrequencyBand _currentARC210FrequencyBand;
        private ARC210FrequencyBand _tempARC210FrequencyBand;
        private readonly string _dcsbiosIdentifier;
        private string _cockpitFrequency;
        private readonly int _frequencyBandSkipCount; // must skip, otherwise it is difficult getting correct band
        private int _frequencyBandSkipCounter;
        private ARC210FrequencyBand[] _supportedFrequencyBands;

        internal ARC210(string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands, uint higherChangeRate = 25, int frequencyBandSkipCount = 2)
        {
            _dcsbiosIdentifier = dcsbiosIdentifier.Trim();
            _initialFrequencyBand = initialFrequencyBand;
            _higherChangeRate = higherChangeRate;
            _frequencyBandSkipCount = frequencyBandSkipCount;
            _supportedFrequencyBands = supportedFrequencyBands;
        }

        internal void InitRadio()
        {
            _supportedFrequencyBands = SortFrequencyBand(_supportedFrequencyBands);
            _currentARC210FrequencyBand = _initialFrequencyBand;
            _tempARC210FrequencyBand = _initialFrequencyBand;

            SetDefaultStandbyFromFrequencyBand(_initialFrequencyBand);
            SetCockpitFrequencyFromFrequencyBand(_initialFrequencyBand);
            if (string.IsNullOrEmpty(_dcsbiosIdentifier))
            {
                throw new ArgumentOutOfRangeException($"ARC-210 : DCS-BIOS identifier is null");
            }
            if (_supportedFrequencyBands == null || _supportedFrequencyBands.Length == 0)
            {
                throw new ArgumentOutOfRangeException($"ARC-210 : No supported frequency bands specified.");
            }
            if (!IsFrequencyBandSupported(_currentARC210FrequencyBand))
            {
                throw new ArgumentOutOfRangeException($"ARC-210 : Frequency band {_currentARC210FrequencyBand} is not supported.");
            }
        }

        /* (FM) 30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        internal void BigFrequencyUp(bool changeFaster = false)
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

        internal void BigFrequencyDown(bool changeFaster = false)
        {
            switch (_currentARC210FrequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        if (GetBigFrequencyStandby() <= FM_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= FM_MIN))
                        {
                            SetBigFrequencyStandby(FM_MAX);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        if (GetBigFrequencyStandby() <= VHF1_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= VHF1_MIN))
                        {
                            SetBigFrequencyStandby(VHF1_MAX);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        if (GetBigFrequencyStandby() <= VHF2_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= VHF2_MIN))
                        {
                            SetBigFrequencyStandby(VHF2_MAX);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        if (GetBigFrequencyStandby() <= UHF_MIN || (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= UHF_MIN))
                        {
                            SetBigFrequencyStandby(UHF_MAX);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
            }
        }

        internal void SmallFrequencyUp()
        {
            if (GetSmallFrequencyStandby() >= 975)
            {
                SetSmallFrequencyStandby(0);
                return;
            }

            AddSmallFrequencyStandby(QUART_FREQ_CHANGE_VALUE);
        }

        internal void SmallFrequencyDown()
        {
            if (GetSmallFrequencyStandby() == 0)
            {
                SetSmallFrequencyStandby(975);
                return;
            }
            SubtractSmallFrequencyStandby(QUART_FREQ_CHANGE_VALUE);
        }

        internal string GetStandbyFrequency()
        {
            return GetBigFrequencyStandby() + "." + GetSmallFrequencyStandby().ToString().PadLeft(3, '0').Trim();
        }

        internal string GetCockpitFrequency()
        {
            return _cockpitFrequency.Trim();
        }

        internal string GetFrequencyBandId()
        {
            return ((int)_currentARC210FrequencyBand).ToString();
        }

        internal string GetTemporaryFrequencyBandId()
        {
            return ((int)_tempARC210FrequencyBand).ToString();
        }

        internal void SetCockpitFrequency(string frequency)
        {
            /*
             * A new frequency has been received from DCS-BIOS.
             *
             * a) user has pressed the button to set the frequency
             * b) user has changed the frequency in the cockpit
             */

            if (!IsFrequencyBandSupported(frequency)) return;

            SetStandbyFrequency(_cockpitFrequency);
            _cockpitFrequency = frequency;

            var newBand = GetFrequencyBand(_cockpitFrequency);
            var oldBand = GetFrequencyBand(GetStandbyFrequency());
            if (SwitchFrequencyBands(newBand, oldBand))
            {
                _currentARC210FrequencyBand = newBand;
                _tempARC210FrequencyBand = newBand;
            }
        }

        private void SetStandbyFrequency(string frequency)
        {
            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            SetBigFrequencyStandby(uint.Parse(array[0]));
            SetSmallFrequencyStandby(uint.Parse(array[1]));
        }

        internal string GetDCSBIOSCommand()
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

        internal void TemporaryFrequencyBandUp()
        {
            _frequencyBandSkipCounter++;
            if (_frequencyBandSkipCounter < _frequencyBandSkipCount)
            {
                return;
            }

            _frequencyBandSkipCounter = 0;

            for (var i = 0; i < _supportedFrequencyBands.Length; i++)
            {
                //   E
                // C E H K     O    X
                // 0 1 2 3     4    5
                if (_tempARC210FrequencyBand == _supportedFrequencyBands[i] && i < _supportedFrequencyBands.Length - 1)
                {
                    _tempARC210FrequencyBand = _supportedFrequencyBands[i + 1];
                    break;
                }

                //       K
                // C E H K
                // 0 1 2 3
                if (_tempARC210FrequencyBand == _supportedFrequencyBands[i] && i >= _supportedFrequencyBands.Length - 1)
                {
                    _tempARC210FrequencyBand = _supportedFrequencyBands[0];
                    break;
                }
            }
        }

        internal void TemporaryFrequencyBandDown()
        {
            _frequencyBandSkipCounter++;
            if (_frequencyBandSkipCounter < _frequencyBandSkipCount)
            {
                return;
            }

            _frequencyBandSkipCounter = 0;

            for (var i = 0; i < _supportedFrequencyBands.Length; i++)
            {
                //   E
                // C E H K     O    X
                // 0 1 2 3     4    5
                if (_tempARC210FrequencyBand == _supportedFrequencyBands[i] && i > 0)
                {
                    _tempARC210FrequencyBand = _supportedFrequencyBands[i - 1];
                    break;
                }

                // C
                // C E H K
                // 0 1 2 3
                if (_tempARC210FrequencyBand == _supportedFrequencyBands[i] && i == 0)
                {
                    _tempARC210FrequencyBand = _supportedFrequencyBands[^1];
                    break;
                }
            }
        }

        public void SwitchFrequencyBand()
        {
            SwitchFrequencyBands(_tempARC210FrequencyBand, _currentARC210FrequencyBand);
            _currentARC210FrequencyBand = _tempARC210FrequencyBand;
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
                        _savedBigFrequencyPerBand[FM] = GetBigFrequencyStandby();
                        _savedSmallFrequencyPerBand[FM] = GetSmallFrequencyStandby();
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        _savedBigFrequencyPerBand[VHF1] = GetBigFrequencyStandby();
                        _savedSmallFrequencyPerBand[VHF1] = GetSmallFrequencyStandby();
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        _savedBigFrequencyPerBand[VHF2] = GetBigFrequencyStandby();
                        _savedSmallFrequencyPerBand[VHF2] = GetSmallFrequencyStandby();
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        _savedBigFrequencyPerBand[UHF] = GetBigFrequencyStandby();
                        _savedSmallFrequencyPerBand[UHF] = GetSmallFrequencyStandby();
                        break;
                    }
            }

            switch (newFrequencyBandBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[FM]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[FM]);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[VHF1]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[VHF1]);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[VHF2]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[VHF2]);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[UHF]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[UHF]);
                        break;
                    }
            }

            VerifyStandbyFrequencyBand();
            return true;
        }

        private bool SetDefaultStandbyFromFrequencyBand(ARC210FrequencyBand frequencyBand)
        {
            switch (frequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[FM]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[FM]);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[VHF1]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[VHF1]);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[VHF2]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[VHF2]);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        SetBigFrequencyStandby(_savedBigFrequencyPerBand[UHF]);
                        SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[UHF]);
                        break;
                    }
            }

            return true;
        }

        private bool SetCockpitFrequencyFromFrequencyBand(ARC210FrequencyBand frequencyBand)
        {
            switch (frequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        _cockpitFrequency = $"{_savedBigFrequencyPerBand[FM]}.{_savedSmallFrequencyPerBand[FM]}".PadRight(7, '0');
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        _cockpitFrequency = $"{_savedBigFrequencyPerBand[VHF1]}.{_savedSmallFrequencyPerBand[VHF1]}".PadRight(7, '0');
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        _cockpitFrequency = $"{_savedBigFrequencyPerBand[VHF2]}.{_savedSmallFrequencyPerBand[VHF2]}".PadRight(7, '0');
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        _cockpitFrequency = $"{_savedBigFrequencyPerBand[UHF]}.{_savedSmallFrequencyPerBand[UHF]}".PadRight(7, '0');
                        break;
                    }
            }

            return true;
        }

        private void VerifyStandbyFrequencyBand()
        {
            if (!IsFrequencyBandSupported(GetFrequencyBand(GetStandbyFrequency())))
            {
                throw new ArgumentOutOfRangeException($"ARC-210:VerifyFrequencyBand => Frequency band {GetFrequencyBand(GetStandbyFrequency())} (Standby = {GetStandbyFrequency()}) is not supported.");
            }
        }

        private uint GetBigFrequencyCockpit()
        {
            return uint.Parse(_cockpitFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries)[0]);
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
            return _supportedFrequencyBands.Any(supportedBand => frequencyBand == supportedBand);
        }

        private bool IsFrequencyBandSupported(string frequency)
        {
            return _supportedFrequencyBands.Any(supportedBand => GetFrequencyBand(frequency) == supportedBand);
        }

        internal ARC210FrequencyBand[] SupportedFrequencyBands()
        {
            return _supportedFrequencyBands;
        }

        private ARC210FrequencyBand[] SortFrequencyBand(ARC210FrequencyBand[] frequencyBand)
        {
            var result = new ARC210FrequencyBand[frequencyBand.Distinct().Count()];
            var index = 0;
            if (frequencyBand.Contains(ARC210FrequencyBand.FM))
            {
                result[index++] = ARC210FrequencyBand.FM;
            }
            if (frequencyBand.Contains(ARC210FrequencyBand.VHF1))
            {
                result[index++] = ARC210FrequencyBand.VHF1;
            }
            if (frequencyBand.Contains(ARC210FrequencyBand.VHF2))
            {
                result[index++] = ARC210FrequencyBand.VHF2;
            }
            if (frequencyBand.Contains(ARC210FrequencyBand.UHF))
            {
                result[index] = ARC210FrequencyBand.UHF;
            }

            return result;
        }

    }
}
