using NonVisuals.Helpers;
using NonVisuals.Radios.RadioControls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NonVisuals.Radios.RadioSettings
{
    public class FlightRadioSettings
    {
        private const int ARRAY_LENGTH = 4;
        private FlightRadioFrequencyBand _initialFrequencyBand;
        private FlightRadioFrequencyBand[] _supportedFrequencyBands;
        private readonly string _dcsbiosIdentifier;
        private readonly uint[] _lowIntegerFrequencyBounds;
        private readonly uint[] _highIntegerFrequencyBounds;
        private readonly uint[] _lowDecimalFrequencyBounds;
        private readonly uint[] _highDecimalFrequencyBounds;
        private readonly uint[] _integerChangeRates;
        private readonly uint[] _integerHighChangeRates;
        private readonly uint[] _decimalChangeRates;
        private readonly uint[] _decimalHighChangeRates;
        private readonly ClickSkipper _clickSkipperForFrequencyBandChanges;
        private readonly ClickSkipper[] _integerFrequencySkippers;




        /// <summary>
        /// Constructs a new object holding the settings for the FlightRadio.
        /// </summary>
        /// <param name="initialFrequencyBand">The Frequency Band the radio should use when initialized</param>
        /// <param name="supportedFrequencyBands">Supported frequency bands</param>
        /// <param name="dcsbiosIdentifier">DCS-BIOS identifier for the radio</param>
        /// <param name="lowIntegerFrequencyBounds">Lowest integer frequency per frequency band</param>
        /// <param name="highIntegerFrequencyBounds">Highest integer frequency per frequency band</param>
        /// <param name="lowDecimalFrequencyBounds">Lowest decimal frequency per frequency band</param>
        /// <param name="highDecimalFrequencyBounds">Highest decimal frequency per frequency band</param>
        /// <param name="integerChangeRates">Change rates for integer frequency per frequency band</param>
        /// <param name="integerHighChangeRates">High change rates for integer frequency per frequency band</param>
        /// <param name="decimalChangeRates">Change rates for decimal frequency per frequency band</param>
        /// <param name="decimalHighChangeRates">High change rates for decimal frequency per frequency band</param>
        /// <param name="skipCountForFrequencyBandChanges">Click skip count while changing frequency band</param>
        /// <param name="integerFrequencySkippers">ClickSkippers for integer decimal as some are too sensitive</param>
        public FlightRadioSettings(
            FlightRadioFrequencyBand initialFrequencyBand,
            FlightRadioFrequencyBand[] supportedFrequencyBands, 
            string dcsbiosIdentifier, 
            uint[] lowIntegerFrequencyBounds, 
            uint[] highIntegerFrequencyBounds, 
            uint[] lowDecimalFrequencyBounds,
            uint[] highDecimalFrequencyBounds, 
            uint[] integerChangeRates, 
            uint[] integerHighChangeRates, 
            uint[] decimalChangeRates, 
            uint[] decimalHighChangeRates,
            int skipCountForFrequencyBandChanges, 
            ClickSkipper[] integerFrequencySkippers)
        {
            _initialFrequencyBand = initialFrequencyBand;
            _supportedFrequencyBands = supportedFrequencyBands;
            _dcsbiosIdentifier = dcsbiosIdentifier;
            _lowIntegerFrequencyBounds = lowIntegerFrequencyBounds;
            _highIntegerFrequencyBounds = highIntegerFrequencyBounds;
            _lowDecimalFrequencyBounds = lowDecimalFrequencyBounds;
            _highDecimalFrequencyBounds = highDecimalFrequencyBounds;
            _integerChangeRates = integerChangeRates;
            _integerHighChangeRates = integerHighChangeRates;
            _decimalChangeRates = decimalChangeRates;
            _decimalHighChangeRates = decimalHighChangeRates;
            _clickSkipperForFrequencyBandChanges = new ClickSkipper(skipCountForFrequencyBandChanges);
            _integerFrequencySkippers = integerFrequencySkippers;
        }
        private FlightRadioFrequencyBand[] SortFrequencyBand(FlightRadioFrequencyBand[] frequencyBand)
        {
            var result = new FlightRadioFrequencyBand[frequencyBand.Distinct().Count()];
            var index = 0;
            if (frequencyBand.Contains(FlightRadioFrequencyBand.HF))
            {
                result[index++] = FlightRadioFrequencyBand.HF;
            }
            if (frequencyBand.Contains(FlightRadioFrequencyBand.VHF1))
            {
                result[index++] = FlightRadioFrequencyBand.VHF1;
            }
            if (frequencyBand.Contains(FlightRadioFrequencyBand.VHF2))
            {
                result[index++] = FlightRadioFrequencyBand.VHF2;
            }
            if (frequencyBand.Contains(FlightRadioFrequencyBand.UHF))
            {
                result[index] = FlightRadioFrequencyBand.UHF;
            }

            return result;
        }

        private void CheckBandBoundsOrder(string name, uint[] array)
        {
            var lastValue = 1;
            foreach (var u in array)
            {
                if (lastValue >= u && u != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(array), $@"Array ({name}) values are not ordered.");
                }

                lastValue = u == 0 ? lastValue : (int)u;
            }
        }

        private void CheckFrequencyLowAndHighOrder(uint[] lowIntegerFrequencies, uint[] highIntegerFrequencies)
        {
            for (var i = 0; i < lowIntegerFrequencies.Length; i++)
            {
                if (lowIntegerFrequencies[i] >= highIntegerFrequencies[i] && lowIntegerFrequencies[i] != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(lowIntegerFrequencies), $@"Frequency Band {(FlightRadioFrequencyBand)i} either integer or decimal lower frequency is higher or equal to higher frequency.");
                }
            }
        }

        public void VerifySettings()
        {
            if (string.IsNullOrEmpty(_dcsbiosIdentifier))
            {
                throw new ArgumentOutOfRangeException(nameof(_dcsbiosIdentifier), @"FlightRadioSettings : DCS-BIOS Identifier is null.");
            }

            if (_lowIntegerFrequencyBounds == null || _lowIntegerFrequencyBounds.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_lowIntegerFrequencyBounds), @"FlightRadioSettings : Integer Low Frequency Bounds are empty.");
            }
            if (_highIntegerFrequencyBounds == null || _highIntegerFrequencyBounds.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_highIntegerFrequencyBounds), @"FlightRadioSettings : Integer High Frequency Bounds are empty.");
            }

            if (_lowDecimalFrequencyBounds == null || _lowDecimalFrequencyBounds.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_lowDecimalFrequencyBounds), @"FlightRadioSettings : Decimal Low Frequency Bounds are empty.");
            }
            if (_highDecimalFrequencyBounds == null || _highDecimalFrequencyBounds.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_highDecimalFrequencyBounds), @"FlightRadioSettings : Decimal High Frequency Bounds are empty.");
            }

            if (_integerChangeRates == null || _integerChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_integerChangeRates), @"FlightRadioSettings : Integer Change Rates are empty.");
            }
            if (_integerHighChangeRates == null || _integerHighChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_integerHighChangeRates), @"FlightRadioSettings : Integer High Change Rates are empty.");
            }

            if (_decimalChangeRates == null || _decimalChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_decimalChangeRates), @"FlightRadioSettings : Decimal Change Rates are empty.");
            }
            if (_decimalHighChangeRates == null || _decimalHighChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_decimalHighChangeRates), @"FlightRadioSettings : Decimal High Change Rates are empty.");
            }

            if (_supportedFrequencyBands == null || _supportedFrequencyBands.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_supportedFrequencyBands), @"FlightRadioSettings : Supported Frequencies Bands are empty.");
            }
            
            if (_integerFrequencySkippers == null || _integerFrequencySkippers.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_integerFrequencySkippers), @"FlightRadioSettings : Integer Frequency Skippers are empty.");
            }

            foreach (var frequencyBand in _supportedFrequencyBands)
            {
                if (_integerChangeRates[(int)frequencyBand] == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(_integerChangeRates), @"FlightRadioSettings : Integer Frequency Change Rate can not be 0.");
                }
                if (_integerHighChangeRates[(int)frequencyBand] == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(_integerHighChangeRates), @"FlightRadioSettings : Integer Frequency High Change Rate can not be 0.");
                }
                if (_decimalChangeRates[(int)frequencyBand] == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(_decimalChangeRates), @"FlightRadioSettings : Decimal Frequency Change Rate can not be 0.");
                }
                if (_decimalHighChangeRates[(int)frequencyBand] == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(_decimalHighChangeRates), @"FlightRadioSettings : Decimal Frequency High Change Rate can not be 0.");
                }
            }

            CheckArrayCounts();
            CheckBandBoundsOrder("_lowIntegerFrequencyBounds", _lowIntegerFrequencyBounds);
            CheckBandBoundsOrder("_highIntegerFrequencyBounds", _highIntegerFrequencyBounds);
            CheckFrequencyLowAndHighOrder(_lowIntegerFrequencyBounds, _highIntegerFrequencyBounds);
            CheckFrequencyLowAndHighOrder(_lowDecimalFrequencyBounds, _highDecimalFrequencyBounds);
            _supportedFrequencyBands = SortFrequencyBand(_supportedFrequencyBands);
        }

        private void CheckArrayCounts()
        {
            var list = new List<KeyValuePair<string, uint[]>>
            {
                new("_lowIntegerFrequencyBounds",_lowIntegerFrequencyBounds),
                new("_highIntegerFrequencyBounds",_highIntegerFrequencyBounds),
                new("_lowDecimalFrequencyBounds",_lowDecimalFrequencyBounds),
                new("_highDecimalFrequencyBounds",_highDecimalFrequencyBounds),
                new("_integerChangeRates",_integerChangeRates),
                new("_integerHighChangeRates",_integerHighChangeRates),
                new("_decimalChangeRates",_decimalChangeRates),
                new("_decimalHighChangeRates",_decimalHighChangeRates)
            };

            foreach (var kvp in list.Where(o => o.Value.Length != ARRAY_LENGTH))
            {
                throw new ArgumentOutOfRangeException(nameof(kvp), $@"FlightRadioSettings : Array count is wrong for {kvp.Key} ({kvp.Value}). All arrays must be {ARRAY_LENGTH} long.");
            }

            if (_integerFrequencySkippers.Length != ARRAY_LENGTH)
            {
                throw new ArgumentOutOfRangeException(nameof(_integerFrequencySkippers), $@"FlightRadioSettings : Array count is wrong for _integerFrequencySkipper ({_integerFrequencySkippers.Length}). All arrays must be {ARRAY_LENGTH} long.");
            }
        }

        public uint[] LowIntegerFrequencyBounds => _lowIntegerFrequencyBounds;

        public uint[] HighIntegerFrequencyBounds => _highIntegerFrequencyBounds;

        public uint[] LowDecimalFrequencyBounds => _lowDecimalFrequencyBounds;

        public uint[] HighDecimalFrequencyBounds => _highDecimalFrequencyBounds;

        public uint[] IntegerChangeRates => _integerChangeRates;

        public uint[] IntegerHighChangeRates => _integerHighChangeRates;

        public uint[] DecimalChangeRates => _decimalChangeRates;

        public uint[] DecimalHighChangeRates => _decimalHighChangeRates;

        public ClickSkipper[] IntegerFrequencySkippers => _integerFrequencySkippers;

        public FlightRadioFrequencyBand[] SupportedFrequencyBands => _supportedFrequencyBands;

        public FlightRadioFrequencyBand InitialFrequencyBand => _initialFrequencyBand;

        public ClickSkipper FrequencyBandSkipper => _clickSkipperForFrequencyBandChanges;

        public string DCSBIOSIdentifier => _dcsbiosIdentifier;
    }
}
