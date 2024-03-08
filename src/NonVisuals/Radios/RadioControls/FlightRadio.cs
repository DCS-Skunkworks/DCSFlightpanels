using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NonVisuals.Radios.RadioSettings;

namespace NonVisuals.Radios.RadioControls
{
    public enum FlightRadioFrequencyBand
    {
        HF = 0,
        VHF1 = 1,
        VHF2 = 2,
        UHF = 3
    }

    public class FlightRadio
    {
        private uint _integerFrequencyStandby;
        private uint _decimalFrequencyStandby;
        private string _cockpitFrequency;
        private readonly uint[] _savedCockpitIntegerFrequencyPerBand = new uint[4];
        private readonly uint[] _savedCockpitDecimalFrequencyPerBand = new uint[4];
        private readonly uint[] _savedIntegerFrequencyPerBand = new uint[4];
        private readonly uint[] _savedDecimalFrequencyPerBand = new uint[4];
        private FlightRadioFrequencyBand _activeFrequencyBand;
        private FlightRadioFrequencyBand _tempFrequencyBand;
        private readonly FlightRadioSettings _settings;
        private bool _isInitialized;

        public FlightRadio(FlightRadioSettings flightRadioSettings)
        {
            _settings = flightRadioSettings;
        }

        public void InitRadio()
        {
            _isInitialized = true;
            _settings.VerifySettings();
            _activeFrequencyBand = _settings.InitialFrequencyBand;
            _tempFrequencyBand = _settings.InitialFrequencyBand;
            PopulateSavedValues();
            FetchStandbyFrequencyFromArray(_settings.InitialFrequencyBand);
            _cockpitFrequency = FetchCockpitFrequencyFromArray(_settings.InitialFrequencyBand);
        }

        private void VerifyIsInitialized()
        {
            if (_isInitialized) return;

            throw new InvalidOperationException($"FlightRadio.InitRadio() for {_settings.DCSBIOSIdentifier} has not been called.");
        }

        /// <summary>
        /// Move up whole number part of standby frequency
        /// </summary>
        /// <param name="changeFaster"></param>
        public void IntegerFrequencyUp(bool changeFaster = false)
        {
            VerifyIsInitialized();

            if (_settings.IntegerFrequencySkippers[(int)_activeFrequencyBand].ShouldSkip()) return;

            if (GetIntegerFrequencyStandby() >= _settings.HighIntegerFrequencyBounds[(int)_activeFrequencyBand] ||
                (changeFaster && GetIntegerFrequencyStandby() + _settings.IntegerHighChangeRates[(int)_activeFrequencyBand] >= _settings.HighIntegerFrequencyBounds[(int)_activeFrequencyBand]))
            {
                SetIntegerFrequencyStandby(_settings.LowIntegerFrequencyBounds[(int)_activeFrequencyBand]);
                return;
            }

            AddIntegerFrequencyStandby(changeFaster ? _settings.IntegerHighChangeRates[(int)_activeFrequencyBand] : _settings.IntegerChangeRates[(int)_activeFrequencyBand]);
        }

        /// <summary>
        /// Move down whole number part of standby frequency
        /// </summary>
        /// <param name="changeFaster"></param>
        public void IntegerFrequencyDown(bool changeFaster = false)
        {
            VerifyIsInitialized();

            if (_settings.IntegerFrequencySkippers[(int)_activeFrequencyBand].ShouldSkip()) return;

            if (GetIntegerFrequencyStandby() <= _settings.LowIntegerFrequencyBounds[(int)_activeFrequencyBand] ||
                (changeFaster && GetIntegerFrequencyStandby() - _settings.IntegerHighChangeRates[(int)_activeFrequencyBand] <= _settings.LowIntegerFrequencyBounds[(int)_activeFrequencyBand]))
            {
                SetIntegerFrequencyStandby(_settings.HighIntegerFrequencyBounds[(int)_activeFrequencyBand]);
                return;
            }

            SubtractIntegerFrequencyStandby(changeFaster ? _settings.IntegerHighChangeRates[(int)_activeFrequencyBand] : _settings.IntegerChangeRates[(int)_activeFrequencyBand]);
        }

        /// <summary>
        /// Move up decimal part of standby frequency
        /// </summary>
        /// <param name="changeFaster"></param>
        public void DecimalFrequencyUp(bool changeFaster = false)
        {
            VerifyIsInitialized();

            if (GetDecimalFrequencyStandby() >= _settings.HighDecimalFrequencyBounds[(int)_activeFrequencyBand] ||
                (changeFaster && GetDecimalFrequencyStandby() + _settings.DecimalHighChangeRates[(int)_activeFrequencyBand] >= _settings.HighDecimalFrequencyBounds[(int)_activeFrequencyBand]))
            {
                SetDecimalFrequencyStandby(_settings.LowDecimalFrequencyBounds[(int)_activeFrequencyBand]);
                return;
            }

            AddDecimalFrequencyStandby(changeFaster ? _settings.DecimalHighChangeRates[(int)_activeFrequencyBand] : _settings.DecimalChangeRates[(int)_activeFrequencyBand]);
        }

        /// <summary>
        /// Move down decimal part of standby frequency
        /// </summary>
        /// <param name="changeFaster"></param>
        public void DecimalFrequencyDown(bool changeFaster = false)
        {
            VerifyIsInitialized();

            if (GetDecimalFrequencyStandby() <= _settings.LowDecimalFrequencyBounds[(int)_activeFrequencyBand] ||
                (changeFaster && GetDecimalFrequencyStandby() - _settings.DecimalHighChangeRates[(int)_activeFrequencyBand] <= _settings.LowDecimalFrequencyBounds[(int)_activeFrequencyBand]))
            {
                SetDecimalFrequencyStandby(_settings.HighDecimalFrequencyBounds[(int)_activeFrequencyBand]);
                return;
            }

            SubtractDecimalFrequencyStandby(changeFaster ? _settings.DecimalHighChangeRates[(int)_activeFrequencyBand] : _settings.DecimalChangeRates[(int)_activeFrequencyBand]);
        }

        public string StandbyFrequency => GetIntegerFrequencyStandby() + "." + GetDecimalFrequencyStandby().ToString().PadLeft(3, '0').Trim();

        public string CockpitFrequency => _cockpitFrequency.Trim();

        public string ActiveFrequencyBandId => ((int)_activeFrequencyBand).ToString();

        public string TemporaryFrequencyBandId => ((int)_tempFrequencyBand).ToString();

        public FlightRadioFrequencyBand TemporaryFrequencyBand => _tempFrequencyBand;

        public void SetCockpitFrequency(string frequency)
        {
            VerifyIsInitialized();

            Debug.WriteLine(LastFrequencies());
            if (!IsFrequencyBandSupported(frequency)) return;
            if (frequency == CockpitFrequency) return;

            var oldCockpitFrequency = _cockpitFrequency;
            Debug.WriteLine($"Old cockpit : {oldCockpitFrequency}");
            _cockpitFrequency = frequency;

            SaveCockpitFrequencyToArray();
            SetStandbyFrequency(oldCockpitFrequency);
            SaveStandByFrequencyToArray();


            var newBand = GetFrequencyBand(_cockpitFrequency);
            var oldBand = GetFrequencyBand(StandbyFrequency);

            if (newBand != oldBand || newBand != _activeFrequencyBand)
            {
                FetchStandbyFrequencyFromArray(newBand);
                _activeFrequencyBand = newBand;
                _tempFrequencyBand = newBand;
            }
            Debug.WriteLine(LastFrequencies());
        }

        public void SwitchFrequencyBand()
        {
            VerifyIsInitialized();

            if (_tempFrequencyBand == _activeFrequencyBand) return;

            Debug.WriteLine(LastFrequencies());
            SaveCockpitFrequencyToArray();
            SaveStandByFrequencyToArray();
            Debug.WriteLine(LastFrequencies());
            FetchStandbyFrequencyFromArray(_tempFrequencyBand);
            _cockpitFrequency = FetchCockpitFrequencyFromArray(_tempFrequencyBand);
            _activeFrequencyBand = _tempFrequencyBand;
            VerifyStandbyFrequencyBand();
            Debug.WriteLine(LastFrequencies());
        }

        private void SetStandbyFrequency(string frequency)
        {
            VerifyIsInitialized();

            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            SetIntegerFrequencyStandby(uint.Parse(array[0]));
            SetDecimalFrequencyStandby(uint.Parse(array[1]));
        }

        public string GetDCSBIOSCommand()
        {
            return $"{_settings.DCSBIOSIdentifier} {StandbyFrequency}\n";
        }

        private FlightRadioFrequencyBand GetFrequencyBand(string frequency)
        {
            VerifyIsInitialized();

            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var integerFrequencyStandby = uint.Parse(array[0]);

            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.HF] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.HF])
            {
                return FlightRadioFrequencyBand.HF;
            }
            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.VHF1] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.VHF1])
            {
                return FlightRadioFrequencyBand.VHF1;
            }
            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.VHF2] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.VHF2])
            {
                return FlightRadioFrequencyBand.VHF2;
            }
            if (integerFrequencyStandby >= _settings.LowDecimalFrequencyBounds[(int)FlightRadioFrequencyBand.UHF] && integerFrequencyStandby <= _settings.HighIntegerFrequencyBounds[(int)FlightRadioFrequencyBand.UHF])
            {
                return FlightRadioFrequencyBand.UHF;
            }

            throw new Exception("FlightRadio : Frequency not matching any frequency bands.");
        }

        public void TemporaryFrequencyBandUp()
        {
            VerifyIsInitialized();

            if (_settings.SupportedFrequencyBands.Length == 1) return;

            if (_settings.FrequencyBandSkipper.ShouldSkip())
            {
                return;
            }

            for (var i = 0; i < _settings.SupportedFrequencyBands.Length; i++)
            {
                //   E
                // C E H K     O    X
                // 0 1 2 3     4    5
                if (_tempFrequencyBand == _settings.SupportedFrequencyBands[i] && i < _settings.SupportedFrequencyBands.Length - 1)
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

        public void TemporaryFrequencyBandDown()
        {
            VerifyIsInitialized();

            if (_settings.SupportedFrequencyBands.Length == 1) return;

            if (_settings.FrequencyBandSkipper.ShouldSkip())
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
            SetIntegerFrequencyStandby(_savedIntegerFrequencyPerBand[(int)frequencyBand]);
            SetDecimalFrequencyStandby(_savedDecimalFrequencyPerBand[(int)frequencyBand]);
            VerifyStandbyFrequencyBand();
        }

        private void SaveStandByFrequencyToArray()
        {
            SaveStandByFrequencyToArray(StandbyFrequency);
            VerifyStandbyFrequencyBand();
        }

        private void SaveStandByFrequencyToArray(string frequency)
        {
            Debug.WriteLine($"Saving : {frequency}");
            var frequencyBand = GetFrequencyBand(frequency);

            var array = frequency.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var integerFrequency = uint.Parse(array[0]);
            var decimalFrequency = uint.Parse(array[1]);

            _savedIntegerFrequencyPerBand[(int)frequencyBand] = integerFrequency;
            _savedDecimalFrequencyPerBand[(int)frequencyBand] = decimalFrequency;
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
            var integerFrequency = uint.Parse(array[0]);
            var decimalFrequency = uint.Parse(array[1]);

            _savedCockpitIntegerFrequencyPerBand[(int)frequencyBand] = integerFrequency;
            _savedCockpitDecimalFrequencyPerBand[(int)frequencyBand] = decimalFrequency;
        }

        private string FetchCockpitFrequencyFromArray(FlightRadioFrequencyBand frequencyBand)
        {
            return $"{_savedCockpitIntegerFrequencyPerBand[(int)frequencyBand]}.{_savedCockpitDecimalFrequencyPerBand[(int)frequencyBand].ToString().PadLeft(3, '0')}";
        }

        private void VerifyStandbyFrequencyBand()
        {
            if (!IsFrequencyBandSupported(GetFrequencyBand(StandbyFrequency)))
            {
                throw new ArgumentOutOfRangeException($"FlightRadio:VerifyFrequencyBand => Frequency band {GetFrequencyBand(StandbyFrequency)} (Standby = {StandbyFrequency}) is not supported.");
            }
        }

        private uint GetIntegerFrequencyCockpit()
        {
            return uint.Parse(_cockpitFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries)[0]);
        }

        private uint GetDecimalFrequencyCockpit()
        {
            return uint.Parse(_cockpitFrequency.Split('.', StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        private void SetIntegerFrequencyStandby(uint value)
        {
            _integerFrequencyStandby = value;
        }

        private void AddIntegerFrequencyStandby(uint value)
        {
            _integerFrequencyStandby += value;
        }

        private void SubtractIntegerFrequencyStandby(uint value)
        {
            _integerFrequencyStandby -= value;
        }

        private uint GetIntegerFrequencyStandby()
        {
            return _integerFrequencyStandby;
        }

        private void SetDecimalFrequencyStandby(uint value)
        {
            _decimalFrequencyStandby = value;
        }

        private void AddDecimalFrequencyStandby(uint value)
        {
            _decimalFrequencyStandby += value;
        }

        private void SubtractDecimalFrequencyStandby(uint value)
        {
            _decimalFrequencyStandby -= value;
        }

        private uint GetDecimalFrequencyStandby()
        {
            return _decimalFrequencyStandby;
        }

        private bool IsFrequencyBandSupported(FlightRadioFrequencyBand frequencyBand)
        {
            return _settings.SupportedFrequencyBands.Any(supportedBand => frequencyBand == supportedBand);
        }

        private bool IsFrequencyBandSupported(string frequency)
        {
            return _settings.SupportedFrequencyBands.Any(supportedBand => GetFrequencyBand(frequency) == supportedBand);
        }

        public FlightRadioFrequencyBand[] SupportedFrequencyBands()
        {
            return _settings.SupportedFrequencyBands;
        }

        public FlightRadioFrequencyBand ActiveFrequencyBand => _activeFrequencyBand;


        public string GetLastStandbyFrequency(FlightRadioFrequencyBand frequencyBand)
        {
            return _savedIntegerFrequencyPerBand[(int)frequencyBand] + "." + _savedDecimalFrequencyPerBand[(int)frequencyBand].ToString().PadLeft(3, '0');
        }

        private void PopulateSavedValues()
        {
            for (var i = 0; i < _settings.LowIntegerFrequencyBounds.Length; i++)
            {
                _savedIntegerFrequencyPerBand[i] = _settings.LowIntegerFrequencyBounds[i];
                _savedCockpitIntegerFrequencyPerBand[i] = _settings.LowIntegerFrequencyBounds[i];
            }
            for (var i = 0; i < _settings.LowDecimalFrequencyBounds.Length; i++)
            {
                _savedDecimalFrequencyPerBand[i] = _settings.LowDecimalFrequencyBounds[i];
                _savedCockpitDecimalFrequencyPerBand[i] = _settings.LowDecimalFrequencyBounds[i];
            }
        }

        public string LastFrequencies()
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
