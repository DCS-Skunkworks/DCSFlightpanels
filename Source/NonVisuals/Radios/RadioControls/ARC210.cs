using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Diagnostics;
using NonVisuals.Helpers;

namespace NonVisuals.Radios.RadioControls
{
    internal enum ARC210FrequencyBand
    {
        FM = 0,
        VHF1 = 1,
        VHF2 = 2,
        UHF = 3
    }

    internal class ARC210
    {
        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        private uint _arc210BigFrequencyStandby;
        private uint _arc210SmallFrequencyStandby;
        private readonly uint[] _lowerFrequencyBounds = { 30, 108, 118, 225 };
        private readonly uint[] _higherFrequencyBounds = { 87, 115, 173, 399 };
        private readonly uint[] _savedBigFrequencyPerBand = { 30, 108, 118, 225 };
        private readonly uint[] _savedSmallFrequencyPerBand = { 0, 0, 0, 0 };
        private readonly ARC210FrequencyBand _initialFrequencyBand;
        private readonly uint _higherChangeRate;
        private const uint QUART_FREQ_CHANGE_VALUE = 25;
        private const uint BIG_FREQ_CHANGE_VALUE = 1;
        private ARC210FrequencyBand _currentARC210FrequencyBand;
        private ARC210FrequencyBand _tempARC210FrequencyBand;
        private readonly string _dcsbiosIdentifier;
        private string _cockpitFrequency;
        private readonly ClickSkipper _clickSkipperFrequencyBand = new ClickSkipper(0); // must skip, otherwise it is difficult getting correct band
        private ARC210FrequencyBand[] _supportedFrequencyBands;

        internal ARC210(string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands, uint higherChangeRate = 25, int frequencyBandSkipCount = 2)
        {
            _dcsbiosIdentifier = dcsbiosIdentifier.Trim();
            _initialFrequencyBand = initialFrequencyBand;
            _higherChangeRate = higherChangeRate;
            _clickSkipperFrequencyBand.ClicksToSkip = frequencyBandSkipCount;
            _supportedFrequencyBands = supportedFrequencyBands;
        }

        internal void InitRadio()
        {
            if (_supportedFrequencyBands == null || _supportedFrequencyBands.Length == 0)
            {
                throw new ArgumentOutOfRangeException($"ARC-210 : No supported frequency bands specified.");
            }

            _supportedFrequencyBands = SortFrequencyBand(_supportedFrequencyBands);
            _currentARC210FrequencyBand = _initialFrequencyBand;
            _tempARC210FrequencyBand = _initialFrequencyBand;

            FetchStandbyFrequencyFromArray(_initialFrequencyBand);
            FetchCockpitFrequencyFromArray(_initialFrequencyBand);
            if (string.IsNullOrEmpty(_dcsbiosIdentifier))
            {
                throw new ArgumentOutOfRangeException($"ARC-210 : DCS-BIOS identifier is null");
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
                        if (GetBigFrequencyStandby() >= _higherFrequencyBounds[(int)ARC210FrequencyBand.FM]  || 
                            (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= _higherFrequencyBounds[(int)ARC210FrequencyBand.FM]))
                        {
                            SetBigFrequencyStandby(_lowerFrequencyBounds[(int)ARC210FrequencyBand.FM]);
                            break;
                        }

                        AddBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        if (GetBigFrequencyStandby() >= _higherFrequencyBounds[(int)ARC210FrequencyBand.VHF1] || 
                            (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= _higherFrequencyBounds[(int)ARC210FrequencyBand.VHF1]))
                        {
                            SetBigFrequencyStandby(_lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF1]);
                            break;
                        }

                        AddBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        if (GetBigFrequencyStandby() >= _higherFrequencyBounds[(int)ARC210FrequencyBand.VHF2] || 
                            (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= _higherFrequencyBounds[(int)ARC210FrequencyBand.VHF2]))
                        {
                            SetBigFrequencyStandby(_lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF2]);
                            break;
                        }

                        AddBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        if (GetBigFrequencyStandby() >= _higherFrequencyBounds[(int)ARC210FrequencyBand.UHF] || 
                            (changeFaster && GetBigFrequencyStandby() + _higherChangeRate >= _higherFrequencyBounds[(int)ARC210FrequencyBand.UHF]))
                        {
                            SetBigFrequencyStandby(_lowerFrequencyBounds[(int)ARC210FrequencyBand.UHF]);
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
                        if (GetBigFrequencyStandby() <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.FM] || 
                            (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.FM]))
                        {
                            SetBigFrequencyStandby(_higherFrequencyBounds[(int)ARC210FrequencyBand.FM]);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        if (GetBigFrequencyStandby() <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF1] || 
                            (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF1]))
                        {
                            SetBigFrequencyStandby(_higherFrequencyBounds[(int)ARC210FrequencyBand.VHF1]);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        if (GetBigFrequencyStandby() <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF2] || 
                            (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF2]))
                        {
                            SetBigFrequencyStandby(_higherFrequencyBounds[(int)ARC210FrequencyBand.VHF2]);
                            break;
                        }

                        SubtractBigFrequencyStandby(changeFaster ? _higherChangeRate : BIG_FREQ_CHANGE_VALUE);
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        if (GetBigFrequencyStandby() <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.UHF] ||
                            (changeFaster && GetBigFrequencyStandby() - _higherChangeRate <= _lowerFrequencyBounds[(int)ARC210FrequencyBand.UHF]))
                        {
                            SetBigFrequencyStandby(_higherFrequencyBounds[(int)ARC210FrequencyBand.UHF]);
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
            Debug.WriteLine(LastFrequencies());
            if (!IsFrequencyBandSupported(frequency)) return;
            if (frequency == GetCockpitFrequency()) return;

            if (GetFrequencyBand(_cockpitFrequency) != GetFrequencyBand(frequency))
            {
                /*
                 * This is a -very- special case where there hasn't been updates from the cockpit. So _cockpitFrequency
                 * can be of different frequency band even if new frequency and standby frequency are of the same band.
                 */
                SaveStandByFrequencyToArray(_cockpitFrequency); 
                FetchCockpitFrequencyFromArray(GetFrequencyBand(frequency));
            }
            
            var oldCockpitFrequency = _cockpitFrequency;
            Debug.WriteLine($"Old cockpit : {oldCockpitFrequency}");
            _cockpitFrequency = frequency;
            
            SetStandbyFrequency(oldCockpitFrequency);
            SaveStandByFrequencyToArray();

            var newBand = GetFrequencyBand(_cockpitFrequency);
            var oldBand = GetFrequencyBand(GetStandbyFrequency());

            if (newBand != oldBand || newBand != _currentARC210FrequencyBand)
            {
                FetchStandbyFrequencyFromArray(newBand);
                _currentARC210FrequencyBand = newBand;
                _tempARC210FrequencyBand = newBand;
            }
            Debug.WriteLine(LastFrequencies());
        }

        public void SwitchFrequencyBand()
        {
            Debug.WriteLine(LastFrequencies());
            SaveStandByFrequencyToArray();
            FetchStandbyFrequencyFromArray(_tempARC210FrequencyBand);
            _currentARC210FrequencyBand = _tempARC210FrequencyBand;
            VerifyStandbyFrequencyBand();
            Debug.WriteLine(LastFrequencies());
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

            if (bigFrequencyStandby >= _lowerFrequencyBounds[(int)ARC210FrequencyBand.FM] && bigFrequencyStandby <= _higherFrequencyBounds[(int)ARC210FrequencyBand.FM])
            {
                return ARC210FrequencyBand.FM;
            }
            if (bigFrequencyStandby >= _lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF1] && bigFrequencyStandby <= _higherFrequencyBounds[(int)ARC210FrequencyBand.VHF1])
            {
                return ARC210FrequencyBand.VHF1;
            }
            if (bigFrequencyStandby >= _lowerFrequencyBounds[(int)ARC210FrequencyBand.VHF2] && bigFrequencyStandby <= _higherFrequencyBounds[(int)ARC210FrequencyBand.VHF2])
            {
                return ARC210FrequencyBand.VHF2;
            }
            if (bigFrequencyStandby >= _lowerFrequencyBounds[(int)ARC210FrequencyBand.UHF] && bigFrequencyStandby <= _higherFrequencyBounds[(int)ARC210FrequencyBand.UHF])
            {
                return ARC210FrequencyBand.UHF;
            }

            throw new Exception("ARC210 : Frequency not matching any frequency bands.");
        }

        internal void TemporaryFrequencyBandUp()
        {
            if (_clickSkipperFrequencyBand.ShouldSkip())
            {
                return;
            }

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
            if (_clickSkipperFrequencyBand.ShouldSkip())
            {
                return;
            }

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
        
        private void FetchStandbyFrequencyFromArray(ARC210FrequencyBand frequencyBand)
        {
            switch (frequencyBand)
            {
                case ARC210FrequencyBand.FM:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.FM]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.FM]);
                    break;
                }
                case ARC210FrequencyBand.VHF1:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.VHF1]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.VHF1]);
                    break;
                }
                case ARC210FrequencyBand.VHF2:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.VHF2]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.VHF2]);
                    break;
                }
                case ARC210FrequencyBand.UHF:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.UHF]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.UHF]);
                    break;
                }
            }

            VerifyStandbyFrequencyBand();
        }

        private void SaveStandByFrequencyToArray()
        {
            SaveStandByFrequencyToArray(GetStandbyFrequency());
            VerifyStandbyFrequencyBand();
        }

        private void SaveStandByFrequencyToArray(string frequency)
        {
            Debug.WriteLine($"Saving : {frequency}");
            var frequencyBand = GetFrequencyBand(frequency);
            
            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var bigFrequency = uint.Parse(array[0]);
            var smallFrequency = uint.Parse(array[1]);

            switch (frequencyBand)
            {
                case ARC210FrequencyBand.FM:
                    {
                        _savedBigFrequencyPerBand[(int)ARC210FrequencyBand.FM] = bigFrequency;
                        _savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.FM] = smallFrequency;
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        _savedBigFrequencyPerBand[(int)ARC210FrequencyBand.VHF1] = bigFrequency;
                        _savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.VHF1] = smallFrequency;
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        _savedBigFrequencyPerBand[(int)ARC210FrequencyBand.VHF2] = bigFrequency; 
                        _savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.VHF2] = smallFrequency;
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        _savedBigFrequencyPerBand[(int)ARC210FrequencyBand.UHF] = bigFrequency;
                        _savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.UHF] = smallFrequency;
                        break;
                    }
            }
        }
        
        private bool FetchCockpitFrequencyFromArray(ARC210FrequencyBand frequencyBand)
        {
            switch (frequencyBand)
            {
                case ARC210FrequencyBand.FM:
                {
                    _cockpitFrequency = $"{_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.FM]}.{_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.FM].ToString().PadLeft(3, '0')}";
                        break;
                    }
                case ARC210FrequencyBand.VHF1:
                    {
                        _cockpitFrequency = $"{_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.VHF1]}.{_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.VHF1].ToString().PadLeft(3, '0')}";
                        break;
                    }
                case ARC210FrequencyBand.VHF2:
                    {
                        _cockpitFrequency = $"{_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.VHF2]}.{_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.VHF2].ToString().PadLeft(3, '0')}";
                        break;
                    }
                case ARC210FrequencyBand.UHF:
                    {
                        _cockpitFrequency = $"{_savedBigFrequencyPerBand[(int)ARC210FrequencyBand.UHF]}.{_savedSmallFrequencyPerBand[(int)ARC210FrequencyBand.UHF].ToString().PadLeft(3, '0')}";
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

        public ARC210FrequencyBand ActiveFrequencyBand => _currentARC210FrequencyBand;

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

        internal string GetLastStandbyFrequency(ARC210FrequencyBand frequencyBand)
        {
            return _savedBigFrequencyPerBand[(int)frequencyBand] + "." + _savedSmallFrequencyPerBand[(int)frequencyBand].ToString().PadLeft(3, '0');
        }

        internal string LastFrequencies()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\nFM   : " + GetLastStandbyFrequency(ARC210FrequencyBand.FM));
            stringBuilder.AppendLine("VHF1 : " + GetLastStandbyFrequency(ARC210FrequencyBand.VHF1));
            stringBuilder.AppendLine("VHF2 : " + GetLastStandbyFrequency(ARC210FrequencyBand.VHF2));
            stringBuilder.AppendLine("UHF  : " + GetLastStandbyFrequency(ARC210FrequencyBand.UHF));
            return stringBuilder.ToString();
        }
    }
}
