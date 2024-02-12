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
    public class DCSBIOSControlLocatorTests
    {
        //private readonly int _a10C = 5; // A-10C Thunderbolt/II
        //private readonly string _dcsbiosPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json");
        //private readonly string _arc21025KhzSelectorId = "ARC210_25KHZ_SEL";

        [Theory]
        [InlineData("ARC210_25KHZ_SEL", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)] // A-10C Thunderbolt/II
        public void GetControlWithValidJSONPath(string dcsbiosControlId, string jsonPath, int aircraftId)
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

            Assert.Equal(dcsbiosControl.Identifier, dcsbiosControlId);
        }

        [Theory]
        [InlineData("NO_CONTROL_WITH_THIS_ID", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)] // A-10C Thunderbolt/II
        public void GetControlUsingInvalidControlIdentifierShouldThrow(string dcsbiosControlId, string jsonPath, int aircraftId)
        {
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

            Assert.Throws<Exception>(() => DCSBIOSControlLocator.GetControl(dcsbiosControlId));
        }

        [Theory]
        [InlineData("ARC210_25KHZ_SEL", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)] // A-10C Thunderbolt/II
        public void GetControlWithKeyEmulationSet(string dcsbiosControlId, string jsonPath, int aircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            // Since key emulation is set DCSBIOSControlLocator shouldn't return any controls.
            Common.SetEmulationModes(EmulationMode.KeyboardEmulationOnly);
            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(aircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var control = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            Assert.Null(control);
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL")]
        [InlineData(DCSBiosOutputType.StringType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL")]
        public void GetDCSBIOSOutputBasedOnOutputTypeWithValidInputs(DCSBiosOutputType dcsBiosOutputType, string jsonPath, int dcsAircraftId, string dcsbiosControlId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var dcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControlId, dcsBiosOutputType);

            Assert.Equal(dcsbiosOutput.DCSBiosOutputType, dcsBiosOutputType);
        }

        [Theory]
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.IntegerType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.StringType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetDCSBIOSOutputUsingInvalidControlIdentifierShouldThrow(string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType, string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            Assert.Throws<Exception>(() => DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControlId, dcsBiosOutputType));
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, "ARC210_25KHZ_SEL", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        [InlineData(DCSBiosOutputType.StringType, "ARC210_25KHZ_SEL", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetDCSBIOSOutputPerOutputTypeUsingValidControlIdentifier(DCSBiosOutputType dcsBiosOutputType, string dcsbiosControlId, string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var dcsbiosOutput = dcsBiosOutputType switch
            {
                DCSBiosOutputType.IntegerType => DCSBIOSControlLocator.GetUIntDCSBIOSOutput(dcsbiosControlId),
                DCSBiosOutputType.StringType => DCSBIOSControlLocator.GetStringDCSBIOSOutput(dcsbiosControlId),
                _ => null
            };

            Assert.NotNull(dcsbiosOutput);
            Assert.Equal(dcsbiosOutput.DCSBiosOutputType, dcsBiosOutputType);
        }

        [Theory]
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.IntegerType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.StringType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetDCSBIOSOutputPerOutputTypeUsingInvalidControlIdentifierShouldThrow(string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType, string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            switch (dcsBiosOutputType)
            {
                case DCSBiosOutputType.IntegerType:
                    Assert.Throws<Exception>(() => DCSBIOSControlLocator.GetUIntDCSBIOSOutput(dcsbiosControlId));
                    break;
                case DCSBiosOutputType.StringType:
                    Assert.Throws<Exception>(() => DCSBIOSControlLocator.GetStringDCSBIOSOutput(dcsbiosControlId));
                    break;
            }
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", "A-10C.json")]
        public void GetModuleControlsFromJson(string jsonPath, string filename)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var controls = DCSBIOSControlLocator.GetModuleControlsFromJson(filename);

            Assert.NotNull(controls);
            Assert.NotEmpty(controls);
        }

        [Theory]
        [InlineData("LON_SEC_FRAC", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json")]    // CommonData
        [InlineData("_ACFT_NAME", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json")]      // MetaDataStart
        [InlineData("_UPDATE_COUNTER", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json")] // MetaDataEnd
        public void GetMetaControls(string controlId, string jsonPath)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var controls = DCSBIOSControlLocator.GetMetaControls();

            Assert.True(controls.Count(o => o.Identifier == controlId) == 1);
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        [InlineData(DCSBiosOutputType.StringType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetOutputControls(DCSBiosOutputType dcsBiosOutputType, string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();
            Common.SetEmulationModes(EmulationMode.DCSBIOSOutputEnabled);

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var controls = DCSBIOSControlLocator.GetOutputControls(dcsBiosOutputType);
            foreach (var dcsbiosControl in controls)
            {
                var count = dcsbiosControl.Outputs.Count(o => o.OutputDataType == dcsBiosOutputType);
                Assert.True(count > 0);

            }
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        [InlineData(DCSBiosOutputType.StringType, @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetOutputControlsDCSBIOSOutputEnabledFlagNotSet(DCSBiosOutputType dcsBiosOutputType, string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();
            // Since Flag DCSBIOSOutputEnabled is not set DCSBIOSControlLocator returns null.

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var controls = DCSBIOSControlLocator.GetOutputControls(dcsBiosOutputType);
            Assert.Null(controls);
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetInputControls(string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();
            Common.SetEmulationModes(EmulationMode.DCSBIOSInputEnabled);

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var controls = DCSBIOSControlLocator.GetInputControls();
            foreach (var dcsbiosControl in controls)
            {
                var count = dcsbiosControl.Inputs.Count;
                Assert.True(count > 0);

            }
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetOutputControlsDCSBIOSInputEnabledFlagNotSet(string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();
            // Since Flag DCSBIOSInputEnabled is not set DCSBIOSControlLocator returns null.

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var controls = DCSBIOSControlLocator.GetInputControls();
            Assert.Null(controls);
        }

        [Theory]
        [InlineData("FLAPS_SWITCH",@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        [InlineData("SASP_MONITOR_TEST", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        [InlineData("UFC_10", @"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5)]
        public void GetLuaCommand(string controlId, string jsonPath, int dcsAircraftId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();
            Common.SetEmulationModes(EmulationMode.DCSBIOSInputEnabled);

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(dcsAircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var luaCommand = DCSBIOSControlLocator.GetLuaCommand(controlId, false);
            Assert.False(string.IsNullOrEmpty(luaCommand));

            luaCommand = DCSBIOSControlLocator.GetLuaCommand(controlId, true);
            Assert.False(string.IsNullOrEmpty(luaCommand));
        }

    }
}
