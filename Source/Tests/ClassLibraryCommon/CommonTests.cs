using ClassLibraryCommon;
using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Interop;
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
        public void GetRelativePath_ShouldReturn_Expectedvalue(string relativeTo, string path, string expected)
        {
            Assert.Equal(expected, Common.GetRelativePath(relativeTo, path));
        }


        [StaTheory]
        [InlineData(Key.Tab, Key.Tab)]
        [InlineData(Key.Up, Key.Up)]
        [InlineData(Key.X, Key.X)]
        [InlineData(Key.NumPad1, Key.NumPad1)]
        public void RealKey_UnspecificKey_ShouldReturn_UnspecificKey(Key inputKey, Key expectedKey)
        {
            var keyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0,
                           inputKey);
            Assert.Equal(expectedKey, keyEvent.RealKey());
        }

        [StaFact]
        public void RealKey_System_ShouldReturn_SystemKey()
        {
            var keyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0,
                Key.System);
            Assert.Equal(keyEvent.SystemKey, keyEvent.RealKey());
        }

        [StaFact]
        public void RealKey_ImeProcessed_ShouldReturn_ImeProcessedKey()
        {
            var keyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero), 0,
                Key.ImeProcessed);

            Assert.Equal(keyEvent.ImeProcessedKey, keyEvent.RealKey());
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
            EmulationMode.SRSEnabled,
            EmulationMode.SRSEnabled }, 8)]
        [InlineData(new EmulationMode[] {
            EmulationMode.DCSBIOSInputEnabled,
            EmulationMode.DCSBIOSOutputEnabled,
            EmulationMode.SRSEnabled,
            EmulationMode.NS430Enabled }, 27)]
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
        public void EmulationModes_InvalidConfigs_Shoud_RaiseException(EmulationMode[] modes)
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

        [Theory]
        //we want to remove SRSEnabled in those tests
        [InlineData(new EmulationMode[] {
            EmulationMode.DCSBIOSInputEnabled,
            EmulationMode.DCSBIOSOutputEnabled,
            EmulationMode.SRSEnabled,
            EmulationMode.NS430Enabled }, 19)] 
        
        [InlineData(new EmulationMode[] {
            EmulationMode.SRSEnabled }, 0)]
        public void EmulationModes_Clear_ShoudRemove_TheFlag(EmulationMode[] modes, int expectedValueAfterRemove)
        {
            try
            {
                modes.ToList().ForEach(x => Common.SetEmulationModes(x));
                Common.ClearEmulationModesFlag(EmulationMode.SRSEnabled);
                Common.ClearEmulationModesFlag(EmulationMode.SRSEnabled); //A double clear should not affect result
                Assert.Equal(expectedValueAfterRemove, Common.GetEmulationModesFlag());
            }
            finally
            {
                EmulationModes_ResetStaticFlagValuesAfterTest();
            }
        }
    }
}
