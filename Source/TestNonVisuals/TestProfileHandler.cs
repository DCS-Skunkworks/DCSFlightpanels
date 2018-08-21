using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NonVisuals;

namespace TestNonVisuals
{
    [TestClass]
    public class TestProfileHandler
    {
        [TestMethod]
        public void TestMethodLoadProfile()
        {
            var profileHandler = new ProfileHandler(@"USERDIRECTORY$$$###\Saved Games\DCS\Scripts\DCS-BIOS\doc\json");

            profileHandler.LoadProfile(profileHandler.DefaultFile());
        }
    }
}
