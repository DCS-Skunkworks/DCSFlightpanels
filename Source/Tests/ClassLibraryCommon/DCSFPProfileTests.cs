using ClassLibraryCommon;
using System.IO;
using System.Reflection;
using Xunit;

namespace Tests.ClassLibraryCommon
{
    public class DCSFPProfileTests
    {
        private string _dcsBiosJsonFolderPath { 
            get {
                string runningFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(runningFolder, @"ClassLibraryCommon\TestsResources\DCS-Bios_Folder Structure\doc\json");
            } 
        }

        //[Fact]
        //Deactivated because of static nightmare
        //public void FillModulesListFromDcsBios_SouldReturn_Total_NumberOfProfiles_FoundIn_BIOSlua_File_Minus_Two()
        //{
        //    DCSFPProfile.FillModulesListFromDcsBios(_dcsBiosJsonFolderPath);
        //    Assert.Equal(37, DCSFPProfile.DCSBIOSModulesCount);
        //}

        [Fact]
        public void FillModulesListFromDcsBios_Sould_Fill_ModulesList()
        {
            DCSFPProfile.FillModulesListFromDcsBios(_dcsBiosJsonFolderPath);
            Assert.Equal(39, DCSFPProfile.Modules.Count);
        }
    }
}
