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
    public class DCSBIOSInputTests
    {
        //38 UH-1H Huey
        //private readonly int _a10C = 5; // A-10C Thunderbolt/II
        //private readonly string _dcsbiosPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json");
        //private readonly string _arc21025KhzSelectorId = "ARC210_25KHZ_SEL";

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL")] // A-10C Thunderbolt/II
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "FLAPS_SWITCH")]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "UFC_1")]
        public void TestDCSBIOSInputConsume(string jsonPath, int aircraftId, string dcsbiosControlId)
        {
            // !! Invalid path logs to error log but doesn't throw exception to the outside
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(aircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var dcsbiosControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            var dcsbiosInput = new DCSBIOSInput();
            dcsbiosInput.Consume(dcsbiosControl);
            Assert.Equal(dcsbiosControl.Inputs.Count, dcsbiosInput.DCSBIOSInputInterfaces.Count);
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "OXY_SUPPLY", DCSBIOSInputType.ACTION)] // A-10C Thunderbolt/II
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "FLAPS_SWITCH", DCSBIOSInputType.FIXED_STEP)]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "UFC_1", DCSBIOSInputType.SET_STATE)]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ENGINE_THROTTLE_FRICTION", DCSBIOSInputType.VARIABLE_STEP)] 
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 38, "UHF_ARC51", DCSBIOSInputType.SET_STRING)] // UH-1H Huey
        public void TestDCSBIOSInputSetSelectedInputInterface(string jsonPath, int aircraftId, string dcsbiosControlId, DCSBIOSInputType dcsbiosInputType)
        {
            // !! Invalid path logs to error log but doesn't throw exception to the outside
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(aircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var dcsbiosControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            var dcsbiosInput = new DCSBIOSInput();
            dcsbiosInput.Consume(dcsbiosControl);
            dcsbiosInput.SetSelectedInterface(dcsbiosInputType);
            Assert.Equal(dcsbiosInput.SelectedDCSBIOSInterface.Interface, dcsbiosInputType);
        }

    }
}
