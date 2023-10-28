using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Diagnostics;
using NonVisuals.Helpers;

namespace NonVisuals.Radios.RadioControls
{
    internal enum FlightRadioFrequencyBand
    {
        HF = 0,
        VHF1 = 1,
        VHF2 = 2,
        UHF = 3
    }

    internal class FlightRadio
    {
        private uint _bigFrequencyStandby;
        private uint _smallFrequencyStandby;
        private string _cockpitFrequency;
        private uint[] _savedCockpitBigFrequencyPerBand;
        private uint[] _savedCockpitSmallFrequencyPerBand;
        private uint[] _savedBigFrequencyPerBand;
        private uint[] _savedSmallFrequencyPerBand;
        private readonly FlightRadioFrequencyBand _initialFrequencyBand;
        private FlightRadioFrequencyBand _currentFrequencyBand;
        private FlightRadioFrequencyBand _tempFrequencyBand;
        private readonly FlightRadioSettings _settings;

        internal FlightRadio(FlightRadioFrequencyBand initialFrequencyBand, FlightRadioSettings flightRadioSettings)
        {
            _initialFrequencyBand = initialFrequencyBand;
            _settings = flightRadioSettings;
        }

        internal void InitRadio()
        {
            _settings.VerifySettings();
            _currentFrequencyBand = _initialFrequencyBand;
            _tempFrequencyBand = _initialFrequencyBand;

            FetchStandbyFrequencyFromArray(_initialFrequencyBand);
            _cockpitFrequency = FetchCockpitFrequencyFromArray(_initialFrequencyBand);
        }

        internal void BigFrequencyUp(bool changeFaster = false)
        {
            if (GetBigFrequencyStandby() >= _settings.HighIntegerFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetBigFrequencyStandby() + _settings.IntegerHigherChangeRates[(int)_currentFrequencyBand] >= _settings.HighIntegerFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetBigFrequencyStandby(_settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand]);
                return;
            }

            AddBigFrequencyStandby(changeFaster ? _settings.IntegerHigherChangeRates[(int)_currentFrequencyBand] : _settings.IntegerChangeRates[(int)_currentFrequencyBand]);
        }

        internal void BigFrequencyDown(bool changeFaster = false)
        {
            if (GetBigFrequencyStandby() <= _settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetBigFrequencyStandby() - _settings.IntegerHigherChangeRates[(int)_currentFrequencyBand] <= _settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetBigFrequencyStandby(_settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.HF]);
                return;
            }

            SubtractBigFrequencyStandby(changeFaster ? _settings.IntegerHigherChangeRates[(int)_currentFrequencyBand] : _settings.IntegerChangeRates[(int)_currentFrequencyBand]);
        }

        internal void SmallFrequencyUp(bool changeFaster = false)
        {
            if (GetSmallFrequencyStandby() >= _settings.HighDecimalFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetSmallFrequencyStandby() + _settings.DecimalHigherChangeRates[(int)_currentFrequencyBand] >= _settings.HighDecimalFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetSmallFrequencyStandby(_settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand]);
                return;
            }

            AddSmallFrequencyStandby(changeFaster ? _settings.DecimalHigherChangeRates[(int)_currentFrequencyBand] : _settings.DecimalChangeRates[(int)_currentFrequencyBand]);
        }

        internal void SmallFrequencyDown(bool changeFaster = false)
        {
            if (GetSmallFrequencyStandby() <= _settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand] ||
                (changeFaster && GetSmallFrequencyStandby() - _settings.DecimalHigherChangeRates[(int)_currentFrequencyBand] <= _settings.LowDecimalFrequencyBounds[(int)_currentFrequencyBand]))
            {
                SetSmallFrequencyStandby(_settings.HighDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.HF]);
                return;
            }

            SubtractSmallFrequencyStandby(changeFaster ? _settings.DecimalHigherChangeRates[(int)_currentFrequencyBand] : _settings.DecimalChangeRates[(int)_currentFrequencyBand]);
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
            return ((int)_currentFrequencyBand).ToString();
        }

        internal string GetTemporaryFrequencyBandId()
        {
            return ((int)_tempFrequencyBand).ToString();
        }

        internal void SetCockpitFrequency(string frequency)
        {
            Debug.WriteLine(LastFrequencies());
            if (!IsFrequencyBandSupported(frequency)) return;
            if (frequency == GetCockpitFrequency()) return;
            
            var oldCockpitFrequency = _cockpitFrequency;
            Debug.WriteLine($"Old cockpit : {oldCockpitFrequency}");
            _cockpitFrequency = frequency;

            SaveCockpitFrequencyToArray();
            SetStandbyFrequency(oldCockpitFrequency);
            SaveStandByFrequencyToArray();
            

            var newBand = GetFrequencyBand(_cockpitFrequency);
            var oldBand = GetFrequencyBand(GetStandbyFrequency());

            if (newBand != oldBand || newBand != _currentFrequencyBand)
            {
                FetchStandbyFrequencyFromArray(newBand);
                _currentFrequencyBand = newBand;
                _tempFrequencyBand = newBand;
            }
            Debug.WriteLine(LastFrequencies());
        }

        public void SwitchFrequencyBand()
        {
            if (_tempFrequencyBand == _currentFrequencyBand) return;

            Debug.WriteLine(LastFrequencies());
            SaveCockpitFrequencyToArray();
            SaveStandByFrequencyToArray();
            Debug.WriteLine(LastFrequencies());
            FetchStandbyFrequencyFromArray(_tempFrequencyBand);
            _cockpitFrequency = FetchCockpitFrequencyFromArray(_tempFrequencyBand);
            _currentFrequencyBand = _tempFrequencyBand;
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
            return $"{_settings.DCSBIOSIdentifier} {GetStandbyFrequency()}\n";
        }

        private FlightRadioFrequencyBand GetFrequencyBand(string frequency)
        {
            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var bigFrequencyStandby = uint.Parse(array[0]);

            if (bigFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.HF] && bigFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.HF])
            {
                return FlightRadioFrequencyBand.HF;
            }
            if (bigFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.VHF1] && bigFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.VHF1])
            {
                return FlightRadioFrequencyBand.VHF1;
            }
            if (bigFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.VHF2] && bigFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.VHF2])
            {
                return FlightRadioFrequencyBand.VHF2;
            }
            if (bigFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.UHF] && bigFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.UHF])
            {
                return FlightRadioFrequencyBand.UHF;
            }

            throw new Exception("FlightRadio : Frequency not matching any frequency bands.");
        }

        internal void TemporaryFrequencyBandUp()
        {
            if (_settings.FrequencySkipper.ShouldSkip())
            {
                return;
            }

            for (var i = 0; i < _settings.SupportedFrequencyBands.Length; i++)
            {
                //   E
                // C E H K     O    X
                // 0 1 2 3     4    5
                if (_tempFrequencyBand ==  _settings.SupportedFrequencyBands[i] && i < _settings.SupportedFrequencyBands.Length - 1)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[i + 1];
                    break;
                }

                //       K
                // C E H K
                // 0 1 2 3
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i >= _settings.SupportedFrequencyBands.Length - 1)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[0];
                    break;
                }
            }
        }

        internal void TemporaryFrequencyBandDown()
        {
            if (_settings.FrequencySkipper.ShouldSkip())
            {
                return;
            }

            Debug.WriteLine($"Temp Band Before : {_tempFrequencyBand}");
            for (var i = 0; i < _settings.SupportedFrequencyBands.Length; i++)
            {
                //   E
                // C E H K     O    X
                // 0 1 2 3     4    5
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i > 0)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[i - 1];
                    break;
                }

                // C
                // C E H K
                // 0 1 2 3
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i == 0)
                {
                    _tempFrequencyBand = _settings.SupportedFrequencyBands[^1];
                    break;
                }
            }
            Debug.WriteLine($"Temp Band After : {_tempFrequencyBand}");
        }
        
        private void FetchStandbyFrequencyFromArray(FlightRadioFrequencyBand frequencyBand)
        {
            switch (frequencyBand)
            {
                case FlightRadioFrequencyBand.HF:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)FlightRadioFrequencyBand.HF]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)FlightRadioFrequencyBand.HF]);
                    break;
                }
                case FlightRadioFrequencyBand.VHF1:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)FlightRadioFrequencyBand.VHF1]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)FlightRadioFrequencyBand.VHF1]);
                    break;
                }
                case FlightRadioFrequencyBand.VHF2:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)FlightRadioFrequencyBand.VHF2]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)FlightRadioFrequencyBand.VHF2]);
                    break;
                }
                case FlightRadioFrequencyBand.UHF:
                {
                    SetBigFrequencyStandby(_savedBigFrequencyPerBand[(int)FlightRadioFrequencyBand.UHF]);
                    SetSmallFrequencyStandby(_savedSmallFrequencyPerBand[(int)FlightRadioFrequencyBand.UHF]);
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

            _savedBigFrequencyPerBand[(int)frequencyBand] = bigFrequency;
            _savedSmallFrequencyPerBand[(int)frequencyBand] = smallFrequency;
        }

        private void SaveCockpitFrequencyToArray()
        {
            SaveCockpitFrequencyToArray(_cockpitFrequency);
            VerifyStandbyFrequencyBand();
        }

        private void SaveCockpitFrequencyToArray(string frequency)
        {
            Debug.WriteLine($"Saving : {frequency}");
            var frequencyBand = GetFrequencyBand(frequency);

            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var bigFrequency = uint.Parse(array[0]);
            var smallFrequency = uint.Parse(array[1]);
            
            _savedCockpitBigFrequencyPerBand[(int)frequencyBand] = bigFrequency;
            _savedCockpitSmallFrequencyPerBand[(int)frequencyBand] = smallFrequency;
        }
        
        private string FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand frequencyBand)
        {
            return $"{_savedCockpitBigFrequencyPerBand[(int)frequencyBand]}.{_savedCockpitSmallFrequencyPerBand[(int)frequencyBand].ToString().PadLeft(3, '0')}";
        }

        private void VerifyStandbyFrequencyBand()
        {
            if (!IsFrequencyBandSupported(GetFrequencyBand(GetStandbyFrequency())))
            {
                throw new ArgumentOutOfRangeException($"FlightRadio:VerifyFrequencyBand => Frequency band {GetFrequencyBand(GetStandbyFrequency())} (Standby = {GetStandbyFrequency()}) is not supported.");
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
            _bigFrequencyStandby = value;
        }

        private void AddBigFrequencyStandby(uint value)
        {
            _bigFrequencyStandby += value;
        }

        private void SubtractBigFrequencyStandby(uint value)
        {
            _bigFrequencyStandby -= value;
        }

        private uint GetBigFrequencyStandby()
        {
            return _bigFrequencyStandby;
        }

        private void SetSmallFrequencyStandby(uint value)
        {
            _smallFrequencyStandby = value;
        }

        private void AddSmallFrequencyStandby(uint value)
        {
            _smallFrequencyStandby += value;
        }

        private void SubtractSmallFrequencyStandby(uint value)
        {
            _smallFrequencyStandby -= value;
        }

        private uint GetSmallFrequencyStandby()
        {
            return _smallFrequencyStandby;
        }

        private bool IsFrequencyBandSupported(FlightRadioFrequencyBand frequencyBand)
        {
            return _settings.SupportedFrequencyBands.Any(supportedBand => frequencyBand == supportedBand);
        }

        private bool IsFrequencyBandSupported(string frequency)
        {
            return _settings.SupportedFrequencyBands.Any(supportedBand => GetFrequencyBand(frequency) == supportedBand);
        }

        internal FlightRadioFrequencyBand[] SupportedFrequencyBands()
        {
            return _settings.SupportedFrequencyBands;
        }

        public FlightRadioFrequencyBand ActiveFrequencyBand => _currentFrequencyBand;


        internal string GetLastStandbyFrequency(FlightRadioFrequencyBand frequencyBand)
        {
            return _savedBigFrequencyPerBand[(int)frequencyBand] + "." + _savedSmallFrequencyPerBand[(int)frequencyBand].ToString().PadLeft(3, '0');
        }

        internal string LastFrequencies()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\nStandby :");
            stringBuilder.AppendLine("HF   : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.HF));
            stringBuilder.AppendLine("VHF1 : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.VHF1));
            stringBuilder.AppendLine("VHF2 : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.VHF2));
            stringBuilder.AppendLine("UHF  : " + GetLastStandbyFrequency(FlightRadioFrequencyBand.UHF));
            stringBuilder.AppendLine("\nCockpit :");
            stringBuilder.AppendLine("HF   : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.HF));
            stringBuilder.AppendLine("VHF1 : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.VHF1));
            stringBuilder.AppendLine("VHF2 : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.VHF2));
            stringBuilder.AppendLine("UHF  : " + FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand.UHF));
            return stringBuilder.ToString();
        }
    }
}
