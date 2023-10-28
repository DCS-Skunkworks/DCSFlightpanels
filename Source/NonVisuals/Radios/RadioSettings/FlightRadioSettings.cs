using NonVisuals.Helpers;
using NonVisuals.Radios.RadioControls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NonVisuals.Radios.RadioSettings
{
    internal class FlightRadioSettings
    {
        private const int ARRAY_LENGTH = 4;
        private readonly uint[] _lowIntegerFrequencyBounds;
        private readonly uint[] _highIntegerFrequencyBounds;
        private readonly uint[] _lowDecimalFrequencyBounds;
        private readonly uint[] _highDecimalFrequencyBounds;
        private readonly uint[] _integerChangeRates;
        private readonly uint[] _integerHigherChangeRates;
        private readonly uint[] _decimalChangeRates;
        private readonly uint[] _decimalHigherChangeRates;
        private FlightRadioFrequencyBand[] _supportedFrequencyBands;
        private readonly ClickSkipper _clickSkipperForFrequencyBandChanges;
        private readonly ClickSkipper[] _integerFrequencySkippers;

        private readonly string _dcsbiosIdentifier;



        /// <summary>
        /// Constructs a new object holding the settings for the FlightRadio.
        /// </summary>
        /// <param name="supportedFrequencyBands">Supported frequency bands</param>
        /// <param name="dcsbiosIdentifier">DCS-BIOS identifier for the radio</param>
        /// <param name="lowIntegerFrequencyBounds">Lowest integer frequency per frequency band</param>
        /// <param name="highIntegerFrequencyBounds">Highest integer frequency per frequency band</param>
        /// <param name="lowDecimalFrequencyBounds">Lowest decimal frequency per frequency band</param>
        /// <param name="highDecimalFrequencyBounds">Highest decimal frequency per frequency band</param>
        /// <param name="integerChangeRates">Change rates for integer frequency per frequency band</param>
        /// <param name="integerHigherChangeRates">Higher change rates for integer frequency per frequency band</param>
        /// <param name="decimalChangeRates">Change rates for decimal frequency per frequency band</param>
        /// <param name="decimalHigherChangeRates">Higher change rates for decimal frequency per frequency band</param>
        /// <param name="skipCountForFrequencyBandChanges">Click skip count while changing frequency band</param>
        /// <param name="integerFrequencySkippers">ClickSkippers for integer decimal as some are too sensitive</param>
        public FlightRadioSettings(FlightRadioFrequencyBand[] supportedFrequencyBands, 
            string dcsbiosIdentifier, 
            uint[] lowIntegerFrequencyBounds, 
            uint[] highIntegerFrequencyBounds, 
            uint[] lowDecimalFrequencyBounds,
            uint[] highDecimalFrequencyBounds, 
            uint[] integerChangeRates, 
            uint[] integerHigherChangeRates, 
            uint[] decimalChangeRates, 
            uint[] decimalHigherChangeRates,
            int skipCountForFrequencyBandChanges, 
            ClickSkipper[] integerFrequencySkippers)
        {
            _lowIntegerFrequencyBounds = lowIntegerFrequencyBounds;
            _highIntegerFrequencyBounds = highIntegerFrequencyBounds;
            _lowDecimalFrequencyBounds = lowDecimalFrequencyBounds;
            _highDecimalFrequencyBounds = highDecimalFrequencyBounds;
            _integerChangeRates = integerChangeRates;
            _integerHigherChangeRates = integerHigherChangeRates;
            _decimalChangeRates = decimalChangeRates;
            _decimalHigherChangeRates = decimalHigherChangeRates;
            _supportedFrequencyBands = supportedFrequencyBands;
            _clickSkipperForFrequencyBandChanges = new ClickSkipper(skipCountForFrequencyBandChanges);
            _dcsbiosIdentifier = dcsbiosIdentifier;
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
                    throw new ArgumentOutOfRangeException(nameof(array), @"Array is not ordered.");
                }

                lastValue = u == 0 ? lastValue : (int)u;
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
                throw new ArgumentOutOfRangeException(nameof(_lowIntegerFrequencyBounds), @"FlightRadioSettings : Integer Lower Frequency Bounds are empty.");
            }
            if (_highIntegerFrequencyBounds == null || _highIntegerFrequencyBounds.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_highIntegerFrequencyBounds), @"FlightRadioSettings : Integer Higher Frequency Bounds are empty.");
            }

            if (_lowDecimalFrequencyBounds == null || _lowDecimalFrequencyBounds.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_lowDecimalFrequencyBounds), @"FlightRadioSettings : Decimal Lower Frequency Bounds are empty.");
            }
            if (_highDecimalFrequencyBounds == null || _highDecimalFrequencyBounds.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_highDecimalFrequencyBounds), @"FlightRadioSettings : Decimal Higher Frequency Bounds are empty.");
            }

            if (_integerChangeRates == null || _integerChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_integerChangeRates), @"FlightRadioSettings : Integer Change Rates are empty.");
            }
            if (_integerHigherChangeRates == null || _integerHigherChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_integerHigherChangeRates), @"FlightRadioSettings : Integer Higher Change Rates are empty.");
            }

            if (_decimalChangeRates == null || _decimalChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_decimalChangeRates), @"FlightRadioSettings : Decimal Change Rates are empty.");
            }
            if (_decimalHigherChangeRates == null || _decimalHigherChangeRates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_decimalHigherChangeRates), @"FlightRadioSettings : Decimal Higher Change Rates are empty.");
            }

            if (_supportedFrequencyBands == null || _supportedFrequencyBands.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_supportedFrequencyBands), @"FlightRadioSettings : Supported Frequencies Bands are empty.");
            }

            CheckArrayCounts();
            CheckBandBoundsOrder("_lowIntegerFrequencyBounds", _lowIntegerFrequencyBounds);
            CheckBandBoundsOrder("_highIntegerFrequencyBounds", _highIntegerFrequencyBounds);
            _supportedFrequencyBands = SortFrequencyBand(_supportedFrequencyBands);
        }

        private void CheckArrayCounts()
        {
            var list = new List<KeyValuePair<string, uint[]>>()
            {
                new KeyValuePair<string, uint[]>("_lowIntegerFrequencyBounds",_lowIntegerFrequencyBounds),
                new KeyValuePair<string, uint[]>("_highIntegerFrequencyBounds",_highIntegerFrequencyBounds),
                new KeyValuePair<string, uint[]>("_lowDecimalFrequencyBounds",_lowDecimalFrequencyBounds),
                new KeyValuePair<string, uint[]>("_highDecimalFrequencyBounds",_highDecimalFrequencyBounds),
                new KeyValuePair<string, uint[]>("_integerChangeRates",_integerChangeRates),
                new KeyValuePair<string, uint[]>("_integerHigherChangeRates",_integerHigherChangeRates),
                new KeyValuePair<string, uint[]>("_decimalChangeRates",_decimalChangeRates),
                new KeyValuePair<string, uint[]>("_decimalHigherChangeRates",_decimalHigherChangeRates)
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

        public uint[] IntegerHigherChangeRates => _integerHigherChangeRates;

        public uint[] DecimalChangeRates => _decimalChangeRates;

        public uint[] DecimalHigherChangeRates => _decimalHigherChangeRates;

        public ClickSkipper[] IntegerFrequencySkippers => _integerFrequencySkippers;

        public FlightRadioFrequencyBand[] SupportedFrequencyBands => _supportedFrequencyBands;

        public ClickSkipper FrequencyBandSkipper => _clickSkipperForFrequencyBandChanges;

        public string DCSBIOSIdentifier => _dcsbiosIdentifier;
    }
}
