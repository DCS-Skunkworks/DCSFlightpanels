using ClassLibraryCommon;
using NonVisuals;
using NonVisuals.EventArgs;
using NonVisuals.Saitek.Panels;
using Xunit;

namespace Tests.NonVisuals
{
    public class EventHandlerSubscriptionTests
    {
        [Fact]
        public void Event_Attachment_Detachment()
        {
            var gamingPanelSkeleton =
                new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel);
            var switchPanel = new SwitchPanelPZ55(new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));

            AppEventHandler.AttachForwardPanelEventListener(switchPanel);
            AppEventHandler.DetachForwardPanelEventListener(switchPanel);
            
            Assert.False(AppEventHandler.OnForwardPanelEventChangedSubscribed());
        }
    }
}
