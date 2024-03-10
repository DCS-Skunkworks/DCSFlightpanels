using NonVisuals.KeyEmulation;
using Xunit;

namespace NonVisuals.Tests 
{
    public class CommonVirtualKeyTests
    {
        [Theory]
        [InlineData(MEF.VirtualKeyCode.LSHIFT)]
        [InlineData(MEF.VirtualKeyCode.RSHIFT)]
        [InlineData(MEF.VirtualKeyCode.LCONTROL)]
        [InlineData(MEF.VirtualKeyCode.RCONTROL)]
        [InlineData(MEF.VirtualKeyCode.LWIN)]
        [InlineData(MEF.VirtualKeyCode.RWIN)]
        [InlineData(MEF.VirtualKeyCode.END)]
        [InlineData(MEF.VirtualKeyCode.DELETE)]
        [InlineData(MEF.VirtualKeyCode.INSERT)]
        [InlineData(MEF.VirtualKeyCode.HOME)]
        [InlineData(MEF.VirtualKeyCode.LEFT)]
        [InlineData(MEF.VirtualKeyCode.RIGHT)]
        [InlineData(MEF.VirtualKeyCode.UP)]
        [InlineData(MEF.VirtualKeyCode.DOWN)]
        [InlineData(MEF.VirtualKeyCode.DIVIDE)]
        [InlineData(MEF.VirtualKeyCode.MULTIPLY)]
        [InlineData(MEF.VirtualKeyCode.SUBTRACT)]
        [InlineData(MEF.VirtualKeyCode.ADD)]
        [InlineData(MEF.VirtualKeyCode.RETURN)]
        [InlineData(MEF.VirtualKeyCode.NUMLOCK)]
        [InlineData(MEF.VirtualKeyCode.LMENU)]
        [InlineData(MEF.VirtualKeyCode.RMENU)]
        public void CommonVirtualKey_IsModifierKey_Should_Return_True_On_All_ModifierKeys(MEF.VirtualKeyCode vkc)
        {
            Assert.True(CommonVirtualKey.IsModifierKey(vkc));
        }

        [Theory]
        [InlineData(MEF.VirtualKeyCode.VK_NULL)]
        [InlineData(MEF.VirtualKeyCode.OEM_CLEAR)]
        [InlineData(MEF.VirtualKeyCode.ATTN)]
        [InlineData(MEF.VirtualKeyCode.PROCESSKEY)]
        [InlineData(MEF.VirtualKeyCode.VOLUME_UP)]
        [InlineData(MEF.VirtualKeyCode.F1)]
        [InlineData(MEF.VirtualKeyCode.SEPARATOR)]
        [InlineData(MEF.VirtualKeyCode.NUMPAD6)]
        [InlineData(MEF.VirtualKeyCode.VK_Y)]
        //And so on...
        public void CommonVirtualKey_IsModifierKey_Should_Return_false_On_Non_ModifierKeys(MEF.VirtualKeyCode vkc)
        {
            Assert.False(CommonVirtualKey.IsModifierKey(vkc));
        }

        [Theory]
        [InlineData(MEF.VirtualKeyCode.RCONTROL)]
        [InlineData(MEF.VirtualKeyCode.END)]
        [InlineData(MEF.VirtualKeyCode.DELETE)]
        [InlineData(MEF.VirtualKeyCode.INSERT)]
        [InlineData(MEF.VirtualKeyCode.HOME)]
        [InlineData(MEF.VirtualKeyCode.LEFT)]
        [InlineData(MEF.VirtualKeyCode.RIGHT)]
        [InlineData(MEF.VirtualKeyCode.UP)]
        [InlineData(MEF.VirtualKeyCode.DOWN)]
        [InlineData(MEF.VirtualKeyCode.DIVIDE)]
        [InlineData(MEF.VirtualKeyCode.MULTIPLY)]
        [InlineData(MEF.VirtualKeyCode.RETURN)]
        [InlineData(MEF.VirtualKeyCode.NUMLOCK)]
        [InlineData(MEF.VirtualKeyCode.RMENU)]
        public void CommonVirtualKey_IsExtendedKey_Should_Return_True_On_All_ExtendedKey(MEF.VirtualKeyCode vkc)
        {
            Assert.True(CommonVirtualKey.IsExtendedKey(vkc));
        }

        [Theory]
        //Those are ModifierKey that are not extended 
        [InlineData(MEF.VirtualKeyCode.LSHIFT)]
        [InlineData(MEF.VirtualKeyCode.RSHIFT)]
        [InlineData(MEF.VirtualKeyCode.LCONTROL)]
        [InlineData(MEF.VirtualKeyCode.LWIN)]
        [InlineData(MEF.VirtualKeyCode.RWIN)]
        [InlineData(MEF.VirtualKeyCode.SUBTRACT)]
        [InlineData(MEF.VirtualKeyCode.ADD)]
        [InlineData(MEF.VirtualKeyCode.LMENU)]
        
        //Other tests
        [InlineData(MEF.VirtualKeyCode.VK_NULL)]
        [InlineData(MEF.VirtualKeyCode.OEM_CLEAR)]
        [InlineData(MEF.VirtualKeyCode.ATTN)]
        [InlineData(MEF.VirtualKeyCode.PROCESSKEY)]
        [InlineData(MEF.VirtualKeyCode.VOLUME_UP)]
        [InlineData(MEF.VirtualKeyCode.F1)]
        [InlineData(MEF.VirtualKeyCode.SEPARATOR)]
        [InlineData(MEF.VirtualKeyCode.NUMPAD6)]
        [InlineData(MEF.VirtualKeyCode.VK_Y)]
        //And so on...
        public void CommonVirtualKey_IsExtendedKey_Should_Return_False_On_Non_ExtendedKey(MEF.VirtualKeyCode vkc)
        {
            Assert.False(CommonVirtualKey.IsExtendedKey(vkc));
        }
    }
}
