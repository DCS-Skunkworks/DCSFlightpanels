using Microsoft.VisualStudio.TestTools.UnitTesting;
using NonVisuals;

namespace TestNonVisuals
{
    [TestClass]
    public class TestSwitchPanelKey
    {
        [TestMethod]
        public void TestConstructor1()
        {
            var group = 0;
            var mask = 1;
            var isOn = true;
            var switchPanelKey = new SwitchPanelKey(group, mask, isOn, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH);

            Assert.IsNotNull(switchPanelKey);
        }

        [TestMethod]
        public void TestGetSetterMask()
        {
            var group = 0;
            var mask = 1;
            var maskSet = 4;
            var isOn = true;
            var switchPanelKey = new SwitchPanelKey(group, mask, isOn, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH);

            switchPanelKey.Mask = maskSet;
            
            Assert.AreEqual(maskSet, switchPanelKey.Mask);
        }

        [TestMethod]
        public void TestGetSetterGroup()
        {
            var group = 0;
            var groupSet = 4;
            var mask = 1;
            var isOn = true;
            var switchPanelKey = new SwitchPanelKey(group, mask, isOn, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH);

            switchPanelKey.Group = groupSet;
            
            Assert.AreEqual(groupSet, switchPanelKey.Group);
        }

        [TestMethod]
        public void TestGetSetterIsOn()
        {
            var group = 0;
            var mask = 1;
            var isOn = true;
            var isOnSet = false;
            var switchPanelKey = new SwitchPanelKey(group, mask, isOn, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH);

            switchPanelKey.IsOn = isOnSet;

            Assert.AreEqual(isOnSet, switchPanelKey.IsOn);
        }

        [TestMethod]
        public void TestGetSwitchkeys()
        {
            var switchkeys = SwitchPanelKey.GetPanelSwitchKeys();

            Assert.AreNotEqual(0, switchkeys.Count);
        }
    }
}
