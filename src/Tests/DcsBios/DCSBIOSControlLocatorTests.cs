using System;
using System.IO;
using System.Linq;
using ClassLibraryCommon;
using DCS_BIOS.ControlLocator;
using DCS_BIOS.Serialized;
using Xunit;

namespace DCSFPTests.DcsBios
{
    [Collection("Sequential")] // This uses EmulationMode
    public class DCSBIOSControlLocatorTests
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
        public void GetControlWithValidJSONPath(string dcsbiosControlId, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            var dcsbiosControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);

            Assert.Equal(dcsbiosControl.Identifier, dcsbiosControlId);
        }

        [Theory]
        [InlineData("NO_CONTROL_WITH_THIS_ID", 5)] // A-10C Thunderbolt/II
        public void GetControlUsingInvalidControlIdentifierShouldThrow(string dcsbiosControlId, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            Assert.Throws<Exception>(() => DCSBIOSControlLocator.GetControl(dcsbiosControlId));
        }

        [Theory]
        [InlineData("ARC210_25KHZ_SEL", 5)] // A-10C Thunderbolt/II
        public void GetControlWithKeyEmulationSet(string dcsbiosControlId, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            // Since key emulation is set DCSBIOSControlLocator shouldn't return any controls.
            Common.SetEmulationModes(EmulationMode.KeyboardEmulationOnly);

            var control = DCSBIOSControlLocator.GetControl(dcsbiosControlId);
            Assert.Null(control);
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, 5, "ARC210_25KHZ_SEL")]
        [InlineData(DCSBiosOutputType.StringType, 5, "ARC210_25KHZ_SEL")]
        public void GetDCSBIOSOutputBasedOnOutputTypeWithValidInputs(DCSBiosOutputType dcsBiosOutputType, int dcsAircraftId, string dcsbiosControlId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            var dcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControlId, dcsBiosOutputType);

            Assert.Equal(dcsbiosOutput.DCSBiosOutputType, dcsBiosOutputType);
        }

        [Theory]
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.IntegerType, 5)]
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.StringType, 5)]
        public void GetDCSBIOSOutputUsingInvalidControlIdentifierShouldThrow(string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

            Assert.Throws<Exception>(() => DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControlId, dcsBiosOutputType));
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, "ARC210_25KHZ_SEL", 5)]
        [InlineData(DCSBiosOutputType.StringType, "ARC210_25KHZ_SEL", 5)]
        public void GetDCSBIOSOutputPerOutputTypeUsingValidControlIdentifier(DCSBiosOutputType dcsBiosOutputType, string dcsbiosControlId, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

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
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.IntegerType, 5)]
        [InlineData("NO_SUCH_CONTROL_ID", DCSBiosOutputType.StringType, 5)]
        public void GetDCSBIOSOutputPerOutputTypeUsingInvalidControlIdentifierShouldThrow(string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;

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
        [InlineData("A-10C.json")]
        public void GetModuleControlsFromJson(string filename)
        {
            if (!SetBaseParameters()) return;

            var controls = DCSBIOSControlLocator.GetModuleControlsFromJson(filename);

            Assert.NotNull(controls);
            Assert.NotEmpty(controls);
        }

        [Theory]
        [InlineData("LON_SEC_FRAC")]    // CommonData
        [InlineData("_ACFT_NAME")]      // MetaDataStart
        [InlineData("_UPDATE_COUNTER")] // MetaDataEnd
        public void GetMetaControls(string controlId)
        {
            if (!SetBaseParameters()) return;

            var controls = DCSBIOSControlLocator.GetMetaControls();

            Assert.Single(controls.Where(o => o.Identifier == controlId));
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, 5)]
        [InlineData(DCSBiosOutputType.StringType, 5)]
        public void GetOutputControls(DCSBiosOutputType dcsBiosOutputType, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;
            Common.SetEmulationModes(EmulationMode.DCSBIOSOutputEnabled);

            var controls = DCSBIOSControlLocator.GetOutputControls(dcsBiosOutputType);
            foreach (var dcsbiosControl in controls)
            {
                var count = dcsbiosControl.Outputs.Count(o => o.OutputDataType == dcsBiosOutputType);
                Assert.True(count > 0);
            }
        }

        [Theory]
        [InlineData(DCSBiosOutputType.IntegerType, 5)]
        [InlineData(DCSBiosOutputType.StringType, 5)]
        public void GetOutputControlsDCSBIOSOutputEnabledFlagNotSet(DCSBiosOutputType dcsBiosOutputType, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;
            // Since Flag DCSBIOSOutputEnabled is not set DCSBIOSControlLocator returns null.

            var controls = DCSBIOSControlLocator.GetOutputControls(dcsBiosOutputType);
            Assert.Null(controls);
        }

        [Theory]
        [InlineData(5)]
        public void GetInputControls(int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;
            Common.SetEmulationModes(EmulationMode.DCSBIOSInputEnabled);

            var controls = DCSBIOSControlLocator.GetInputControls();
            foreach (var dcsbiosControl in controls)
            {
                var count = dcsbiosControl.Inputs.Count;
                Assert.True(count > 0);

            }
        }

        [Theory]
        [InlineData(5)]
        public void GetOutputControlsDCSBIOSInputEnabledFlagNotSet(int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;
            // Since Flag DCSBIOSInputEnabled is not set DCSBIOSControlLocator returns null.

            var controls = DCSBIOSControlLocator.GetInputControls();
            Assert.Null(controls);
        }

        [Theory]
        [InlineData("FLAPS_SWITCH", 5)]
        [InlineData("SASP_MONITOR_TEST", 5)]
        [InlineData("UFC_10", 5)]
        public void GetLuaCommand(string controlId, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;
            Common.SetEmulationModes(EmulationMode.DCSBIOSInputEnabled);

            var luaCommand = DCSBIOSControlLocator.GetLuaCommand(controlId, false);
            Assert.False(string.IsNullOrEmpty(luaCommand));

            luaCommand = DCSBIOSControlLocator.GetLuaCommand(controlId, true);
            Assert.False(string.IsNullOrEmpty(luaCommand));
        }

        [Theory]
        [InlineData("NO_SUCH_CONTROL_ID", 5)]
        public void GetLuaCommandWithInvalidControlIdentifier(string controlId, int dcsAircraftId)
        {
            if (!SetBaseParameters(dcsAircraftId)) return;
            Common.SetEmulationModes(EmulationMode.DCSBIOSInputEnabled);

            var luaCommand = DCSBIOSControlLocator.GetLuaCommand(controlId, false);
            Assert.True(string.IsNullOrEmpty(luaCommand));
        }

    }
}
