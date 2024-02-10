using System;
using System.IO;
using System.Linq;
using ClassLibraryCommon;
using DCS_BIOS.ControlLocator;
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
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL")] // A-10C Thunderbolt/II
        public void TestLocatorGetControlValidJSONPath(string jsonPath, int aircraftId, string dcsbiosControlId)
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

            var arc21025SelectorControl = DCSBIOSControlLocator.GetControl(dcsbiosControlId);

            Assert.Equal(arc21025SelectorControl.Identifier, dcsbiosControlId);
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "NO_CONTROL_WITH_THIS_ID")] // A-10C Thunderbolt/II
        public void TestLocatorGetControlInvalidControlId(string jsonPath, int aircraftId, string dcsbiosControlId)
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
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL")] // A-10C Thunderbolt/II
        public void TestLocatorGetControlKeyEmulation(string jsonPath, int aircraftId, string dcsbiosControlId)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.SetEmulationModes(EmulationMode.KeyboardEmulationOnly);
            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(aircraftId);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            Assert.Null(() => DCSBIOSControlLocator.GetControl(dcsbiosControlId)); här
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL", DCSBiosOutputType.IntegerType)]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL", DCSBiosOutputType.StringType)]
        public void TestLocatorGetDCSBIOSOutput(string jsonPath, int a10C, string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(a10C);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var arc21025Output = DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControlId, dcsBiosOutputType);

            Assert.Equal(arc21025Output.DCSBiosOutputType, dcsBiosOutputType);
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "NO_SUCH_CONTROL_ID", DCSBiosOutputType.IntegerType)]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "NO_SUCH_CONTROL_ID", DCSBiosOutputType.StringType)]
        public void TestLocatorGetDCSBIOSOutputFail(string jsonPath, int a10C, string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(a10C);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            Assert.Throws<Exception>(() => DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControlId, dcsBiosOutputType));
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL", DCSBiosOutputType.IntegerType)]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "ARC210_25KHZ_SEL", DCSBiosOutputType.StringType)]
        public void TestLocatorGetDCSBIOSOutput2(string jsonPath, int a10C, string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(a10C);

            DCSBIOSControlLocator.DCSAircraft = dcsAircraft;
            DCSBIOSControlLocator.JSONDirectory = jsonPath;

            var arc21025Output = dcsBiosOutputType switch
            {
                DCSBiosOutputType.IntegerType => DCSBIOSControlLocator.GetUIntDCSBIOSOutput(dcsbiosControlId),
                DCSBiosOutputType.StringType => DCSBIOSControlLocator.GetStringDCSBIOSOutput(dcsbiosControlId),
                _ => null
            };

            Assert.NotNull(arc21025Output);
            Assert.Equal(arc21025Output.DCSBiosOutputType, dcsBiosOutputType);
        }

        [Theory]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "NO_SUCH_CONTROL_ID", DCSBiosOutputType.IntegerType)]
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", 5, "NO_SUCH_CONTROL_ID", DCSBiosOutputType.StringType)]
        public void TestLocatorGetDCSBIOSOutput2Fail(string jsonPath, int a10C, string dcsbiosControlId, DCSBiosOutputType dcsBiosOutputType)
        {
            jsonPath = Environment.ExpandEnvironmentVariables(jsonPath);
            if (!Directory.Exists(jsonPath))
            {
                //No need to test as dcs-bios isn't installed
                return;
            }
            Common.ResetEmulationModesFlag();

            DCSAircraft.FillModulesListFromDcsBios(jsonPath, true);
            var dcsAircraft = DCSAircraft.GetAircraft(a10C);

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
        public void TestLocatorGetModuleControlsFromJson(string jsonPath, string filename)
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
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", "LON_SEC_FRAC")]    // CommonData
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", "_ACFT_NAME")]      // MetaDataStart
        [InlineData(@"%USERPROFILE%\Saved Games\DCS\Scripts\DCS-BIOS\doc\json", "_UPDATE_COUNTER")] // MetaDataEnd
        public void TestLocatorGetMetaModules(string jsonPath, string controlId)
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

    }
}
