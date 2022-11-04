using ClassLibraryCommon;
using System;
using System.Linq;
using Xunit;

namespace Tests.ClassLibraryCommon
{
    public class CommonTests
    {
        [Theory]
        [InlineData("123", "123")]
        [InlineData("123 ABCD", "123 ABCD")]
        [InlineData("", "")]
        [InlineData("RMENU + LCONTROL", "RMENU ")]
        [InlineData("XxxX RMENU + LCONTROL YyY", "XxxX RMENU  YyY")]
        [InlineData("XxxXRMENU + LCONTROLYyY", "XxxXRMENU YyY")]
        [InlineData("LCONTROL + RMENU", " RMENU")]
        [InlineData("XxxX LCONTROL + RMENU YyY", "XxxX  RMENU YyY")]
        [InlineData("XxxXLCONTROL + RMENUYyY", "XxxX RMENUYyY")]
        public void RemoveLControl_ShouldReturn_ExpectedString(string inputString, string expected)
        {
            Assert.Equal(expected, Common.RemoveLControl(inputString));
        }


        [Theory]
        [InlineData(@"\\testX", @"\\testX", @".\")]
        [InlineData(@"\\testX\1", @"\\testX\1", @".\")]
        [InlineData(@"\\testX", @"\\testY", @"file:\\testy\")]
        [InlineData(@"\\testX\1", @"\\testY\1", @"file:\\testy\1")]
        [InlineData(@"\\testX\1\2", @"\\testX\1", @"..\1")]
        [InlineData(@"\\testX\1\2\3", @"\\testX\1", @"..\..\1")]
        [InlineData(@"\\testX\1\2\3", @"\\testX\1\2", @"..\2")]
        [InlineData(@"\\testX\1\2\3\", @"\\testX\1\2\", @"..\")]
        [InlineData(@"\\testX\1\2\3", @"\\testX\1\2\", @".\")]
        [InlineData(@"\\testX\1\2\3\", @"\\testX\1\2", @"..\..\2")]
        [InlineData(@"\\testX\1\2\3\file.txt", @"\\testX\1\2\file.txt", @"..\file.txt")]
        [InlineData(@"\\testX\1", @"\\testX\1\2", @"1\2")]
        [InlineData(@"\\testX\1", @"\\testX\1\2\3", @"1\2\3")]
        [InlineData(@"\\testX\1\2", @"\\testX\1\2\3", @"2\3")]
        [InlineData(@"\\testX\1\2\file.txt", @"\\testX\1\2\3\file.txt", @"3\file.txt")]
        public void GetRelativePath_ShouldReturn_ExpectedValue(string relativeTo, string path, string expected)
        {
            Assert.Equal(expected, Common.GetRelativePath(relativeTo, path));
        }

        [Fact]
        public void EmulationModes_Set_ShouldSet_ExpectedValue()
        {
            try
            {
                Common.SetEmulationModes(EmulationMode.DCSBIOSInputEnabled);
                Assert.Equal(1, Common.GetEmulationModesFlag());
            }
            finally
            {
                Common.ResetEmulationModesFlag();
                Assert.Equal(0, Common.GetEmulationModesFlag());
            }
        }
        
        [Fact]
        private void EmulationModes_ResetStaticFlagValuesAfterTest() {
            Common.ResetEmulationModesFlag(); //should always call reset after test 'cause static
            Assert.Equal(0, Common.GetEmulationModesFlag());
        }

        [StaTheory]
        [InlineData(new EmulationMode[] {
            EmulationMode.DCSBIOSInputEnabled,
            EmulationMode.DCSBIOSOutputEnabled }, 3)]
        [InlineData(new EmulationMode[] {
            EmulationMode.DCSBIOSInputEnabled,
            EmulationMode.DCSBIOSOutputEnabled,
            EmulationMode.NS430Enabled }, 19)]
        public void EmulationModes_SetMultipleValue_ShouldSet_ExpectedValue(EmulationMode[] modes, int expectedValue)
        {
            try
            {
                modes.ToList().ForEach(x => Common.SetEmulationModes(x));
                Assert.Equal(expectedValue, Common.GetEmulationModesFlag());
            }
            finally
            {
                EmulationModes_ResetStaticFlagValuesAfterTest();
            }
        }

        [Theory]
        [InlineData(new EmulationMode[] {
            EmulationMode.KeyboardEmulationOnly,
            EmulationMode.DCSBIOSOutputEnabled})]
        [InlineData(new EmulationMode[] {
            EmulationMode.KeyboardEmulationOnly,
            EmulationMode.DCSBIOSInputEnabled})]
        [InlineData(new EmulationMode[] {
            EmulationMode.KeyboardEmulationOnly,
            EmulationMode.DCSBIOSInputEnabled,
            EmulationMode.DCSBIOSOutputEnabled,})]
        public void EmulationModes_InvalidConfigs_Should_RaiseException(EmulationMode[] modes)
        {
            try
            {
                Assert.Throws<Exception>(
                () => modes.ToList().ForEach(x => Common.SetEmulationModes(x))
                );
            }
            finally
            {
                EmulationModes_ResetStaticFlagValuesAfterTest();
            }
        }
    }
}
