using System;
using System.IO;
using System.Linq;
using ClassLibraryCommon;
using DCS_BIOS.ControlLocator;
using DCS_BIOS.Json;
using DCS_BIOS.Serialized;
using Xunit;

namespace DCSFPTests.DcsBios
{
    [Collection("Sequential")] 
    public class DCSBIOSInputTests
    {
        private readonly string _dcsbiosPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json");

        private bool SetBaseParameters()
        {
            // !! Invalid path logs to error log but doesn't throw exception to the outside
            if (!Directory.Exists(_dcsbiosPath))
            {
                //No need to test as dcs-bios isn't installed
                return false;
            }

            DCSBIOSControlLocator.JSONDirectory = _dcsbiosPath;
            Common.ResetEmulationModesFlag();

            return true;
        }

        private bool SetBaseParameters(int dcsAircraftId)
        {
            // !! Invalid path logs to error log but doesn't throw exception to the outside
            if (!Directory.Exists(_dcsbiosPath))
            {
                //No need to test as dcs-bios isn't installed
                return false;
            }

            DCSAircraft.FillModulesListFromDcsBios(_dcsbiosPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = _dcsbiosPath;
            Common.ResetEmulationModesFlag();

            return true;
        }

        [Theory]
        [InlineData("ARC210_25KHZ_SEL", 5)] // A-10C Thunderbolt/II
        [InlineData("FLAPS_SWITCH", 5)]
        [InlineData("UFC_1", 5)]
        public void TestDCSBIOSInputConsume(string dcsbiosControlId, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            var dcsbiosControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            var dcsbiosInput = new DCSBIOSInput();
            dcsbiosInput.Consume(dcsbiosControl);
            Assert.Equal(dcsbiosInput.DCSBIOSInputInterfaces.Count, dcsbiosControl.Inputs.Count);
        }

        [Theory]
        [InlineData(DCSBIOSInputType.ACTION, 5, "OXY_SUPPLY")] // A-10C Thunderbolt/II
        [InlineData(DCSBIOSInputType.FIXED_STEP, 5, "FLAPS_SWITCH")]
        [InlineData(DCSBIOSInputType.SET_STATE, 5, "UFC_1")]
        [InlineData(DCSBIOSInputType.VARIABLE_STEP, 5, "ENGINE_THROTTLE_FRICTION")]
        [InlineData(DCSBIOSInputType.SET_STRING, 38, "UHF_ARC51")] // UH-1H Huey
        public void TestDCSBIOSInputSetSelectedInputInterface(DCSBIOSInputType dcsbiosInputType, int dcsAircraftId, string dcsbiosControlId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            var dcsbiosControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            var dcsbiosInput = new DCSBIOSInput();
            dcsbiosInput.Consume(dcsbiosControl);
            dcsbiosInput.SetSelectedInterface(dcsbiosInputType);
            Assert.Equal(dcsbiosInputType, dcsbiosInput.SelectedDCSBIOSInterface.Interface);
        }

        [Theory]
        [InlineData("Toggle switch state", 5, "OXY_SUPPLY", DCSBIOSInputType.ACTION)] // A-10C Thunderbolt/II
        [InlineData("switch to previous or next state", 5, "FLAPS_SWITCH", DCSBIOSInputType.FIXED_STEP)]
        [InlineData("set position", 5, "UFC_1", DCSBIOSInputType.SET_STATE)]
        [InlineData("turn the dial left or right", 5, "ENGINE_THROTTLE_FRICTION", DCSBIOSInputType.VARIABLE_STEP)]
        [InlineData("The frequency to set, with or without a decimal place", 38, "UHF_ARC51", DCSBIOSInputType.SET_STRING)] // UH-1H Huey
        public void TestDCSBIOSInputGetDescriptionForInterface(string description, int dcsAircraftId, string dcsbiosControlId, DCSBIOSInputType dcsbiosInputType)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            var dcsbiosControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            var dcsbiosInput = new DCSBIOSInput();
            dcsbiosInput.Consume(dcsbiosControl);

            Assert.Equal(description, dcsbiosInput.GetDescriptionForInterface(dcsbiosInputType));
        }

        [Theory]
        [InlineData(0, 5, "OXY_SUPPLY", DCSBIOSInputType.ACTION)] // A-10C Thunderbolt/II
        [InlineData(0, 5, "FLAPS_SWITCH", DCSBIOSInputType.FIXED_STEP)]
        [InlineData(65535, 5, "TACAN_VOL", DCSBIOSInputType.SET_STATE)]
        [InlineData(65535, 5, "ENGINE_THROTTLE_FRICTION", DCSBIOSInputType.VARIABLE_STEP)]
        public void TestDCSBIOSInputGetMaxValueInterface(int maxValue, int dcsAircraftId, string dcsbiosControlId, DCSBIOSInputType dcsbiosInputType)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            var dcsbiosControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            var dcsbiosInput = new DCSBIOSInput();
            dcsbiosInput.Consume(dcsbiosControl);

            Assert.Equal(maxValue, dcsbiosInput.GetMaxValueForInterface(dcsbiosInputType));
        }
    }
}
