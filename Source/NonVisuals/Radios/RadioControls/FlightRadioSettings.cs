using NonVisuals.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NonVisuals.Radios.RadioControls
{
    internal class FlightRadioSettings
    {
        private uint[] _lowIntegerFrequencyBounds;
        private uint[] _highIntegerFrequencyBounds;
        private uint[] _lowDecimalFrequencyBounds;
        private uint[] _highDecimalFrequencyBounds;
        private readonly uint[] _integerChangeRates;
        private readonly uint[] _integerHigherChangeRates;
        private readonly uint[] _decimalChangeRates;
        private readonly uint[] _decimalHigherChangeRates;
        private FlightRadioFrequencyBand[] _supportedFrequencyBands;
        private readonly ClickSkipper _clickSkipperForFrequencyBand;

        private readonly string _dcsbiosIdentifier;



        /// <summary>
        /// Constructs a new object holding the settings for the FlightRadio.
        /// </summary>
        /// <param name="lowIntegerFrequencyBounds">Lowest integer frequency per frequency band</param>
        /// <param name="highIntegerFrequencyBounds">Highest integer frequency per frequency band</param>
        /// <param name="lowDecimalFrequencyBounds">Lowest decimal frequency per frequency band</param>
        /// <param name="highDecimalFrequencyBounds">Highest decimal frequency per frequency band</param>
        /// <param name="integerChangeRates">Change rates for integer frequency per frequency band</param>
        /// <param name="integerHigherChangeRates">Higher change rates for integer frequency per frequency band</param>
        /// <param name="decimalChangeRates">Change rates for decimal frequency per frequency band</param>
        /// <param name="decimalHigherChangeRates">Higher change rates for decimal frequency per frequency band</param>
        /// <param name="skipCountForFrequencyBand">Click skip count while changing frequency band</param>
        /// <param name="supportedFrequencyBands">Supported frequency bands</param>
        /// <param name="dcsbiosIdentifier">DCS-BIOS identifier for the radio</param>
        public FlightRadioSettings(uint[] lowIntegerFrequencyBounds, uint[] highIntegerFrequencyBounds, uint[] lowDecimalFrequencyBounds,
            uint[] highDecimalFrequencyBounds, uint[] integerChangeRates, uint[] integerHigherChangeRates, uint[] decimalChangeRates, uint[] decimalHigherChangeRates,
            FlightRadioFrequencyBand[] supportedFrequencyBands, int skipCountForFrequencyBand, string dcsbiosIdentifier)
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
            _clickSkipperForFrequencyBand = new ClickSkipper(skipCountForFrequencyBand);
            _dcsbiosIdentifier = dcsbiosIdentifier;
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

        private void CheckBandBoundsOrder(ref uint[] array)
        {
            var lastValue = 1;
            foreach (var u in array)
            {
                if (lastValue >= u)
                {
                    throw new ArgumentOutOfRangeException(nameof(array), @"Array is not ordered.");
                }

                lastValue = (int)u;
            }
        }

        public void VerifySettings()
        {
            if (string.IsNullOrEmpty(_dcsbiosIdentifier))
            {
                throw new ArgumentOutOfRangeException(nameof(_dcsbiosIdentifier),@"FlightRadioSettings : DCS-BIOS Identifier is null.");
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
            CheckBandBoundsOrder(ref _lowIntegerFrequencyBounds);
            CheckBandBoundsOrder(ref _highIntegerFrequencyBounds);
            CheckBandBoundsOrder(ref _lowDecimalFrequencyBounds);
            CheckBandBoundsOrder(ref _highDecimalFrequencyBounds);
            _supportedFrequencyBands = SortFrequencyBand(_supportedFrequencyBands);
        }

        private void CheckArrayCounts()
        {
            var count = -1;
            var list = new List<uint[]>
            {
                _lowIntegerFrequencyBounds,
                _highIntegerFrequencyBounds,
                _lowDecimalFrequencyBounds,
                _highDecimalFrequencyBounds,
                _integerChangeRates,
                _integerHigherChangeRates,
                _decimalChangeRates,
                _decimalHigherChangeRates
            };

            foreach (var array in list)
            {
                if (count == -1) count = array.Length;

                if (array.Length != count)
                {
                    throw new ArgumentOutOfRangeException(nameof(array), @"FlightRadioSettings : Array count differs. All arrays must have same number of entries.");
                }
            }

            if (count != _supportedFrequencyBands.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(_supportedFrequencyBands), @"FlightRadioSettings : Array count differs. All arrays must have same number of entries.");
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

        public FlightRadioFrequencyBand[] SupportedFrequencyBands => _supportedFrequencyBands;

        public ClickSkipper FrequencySkipper => _clickSkipperForFrequencyBand;

        public string DCSBIOSIdentifier => _dcsbiosIdentifier;
    }
}
