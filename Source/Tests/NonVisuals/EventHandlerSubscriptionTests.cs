using ClassLibraryCommon;
using DCS_BIOS.EventArgs;
using NonVisuals.EventArgs;
using NonVisuals.HID;
using NonVisuals.Panels.Saitek.Panels;
using NonVisuals.Panels.StreamDeck.Events;
using NonVisuals.Panels.StreamDeck.Panels;
using Xunit;

namespace DCSFPTests.NonVisuals
{
    [Collection("Sequential")]
    public class EventHandlerSubscriptionTests
    {
        /*************************************************************************************************
         *                                          SWITCHPANEL
         *************************************************************************************************/
        [Fact]
        public void Event_SwitchPanel_SettingsModified_Proper_Attachment()
        {
            var gamingPanelSkeleton =
                new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel);
            var switchPanel = new SwitchPanelPZ55(new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));

            //SwitchPanel
            Assert.True(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());

            //GamingPanel
            Assert.True(AppEventHandler.OnForwardPanelEventChangedSubscribed());
            Assert.True(AppEventHandler.OnProfileEventSubscribed());
            Assert.True(AppEventHandler.OnSavePanelSettingsSubscribed());
            Assert.True(AppEventHandler.OnSavePanelSettingsJSONSubscribed());
        }

        //[Fact]
        //public void Event_SwitchPanel_SettingsModified_Proper_Detachment()
        //{
        //    var gamingPanelSkeleton =
        //        new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel);
        //    var switchPanel = new SwitchPanelPZ55(new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
        //    switchPanel.Dispose();

        //    //SwitchPanel
        //    Assert.False(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());

        //    //GamingPanel
        //    Assert.False(AppEventHandler.OnForwardPanelEventChangedSubscribed());
        //    Assert.False(AppEventHandler.OnProfileEventSubscribed());
        //    Assert.False(AppEventHandler.OnSavePanelSettingsSubscribed());
        //    Assert.False(AppEventHandler.OnSavePanelSettingsJSONSubscribed());
        //}


        /*************************************************************************************************
         *                                          MULTIPANEL
         *************************************************************************************************/

        [Fact]
        public void Event_MultiPanel_SettingsModified_Proper_Attachment()
        {
            var gamingPanelSkeleton =
                new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            var multiPanelPZ70 = new MultiPanelPZ70(new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));

            //MultiPanel
            Assert.True(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());
            Assert.True(AppEventHandler.OnForwardPanelEventChangedSubscribed());

            //GamingPanel
            Assert.True(AppEventHandler.OnProfileEventSubscribed());
            Assert.True(AppEventHandler.OnSavePanelSettingsSubscribed());
            Assert.True(AppEventHandler.OnSavePanelSettingsJSONSubscribed());
        }

        //[Fact]
        //public void Event_MultiPanel_SettingsModified_Proper_Detachment()
        //{
        //    var gamingPanelSkeleton =
        //        new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
        //    var multiPanelPZ70 = new MultiPanelPZ70(new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
        //    multiPanelPZ70.Dispose();

        //    //MultiPanel
        //    Assert.False(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());

        //    //GamingPanel
        //    Assert.False(AppEventHandler.OnForwardPanelEventChangedSubscribed());
        //    Assert.False(AppEventHandler.OnProfileEventSubscribed());
        //    Assert.False(AppEventHandler.OnSavePanelSettingsSubscribed());
        //    Assert.False(AppEventHandler.OnSavePanelSettingsJSONSubscribed());
        //}


        /*************************************************************************************************
         *                                          STREAMDECKPANEL
         *************************************************************************************************/

        [Fact]
        public void Event_StreamDeck_OnDirtyConfigurations_Proper_Attachment()
        {
            var gamingPanelSkeleton =
                new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            var streamDeckPanel = new StreamDeckPanel(GamingPanelEnum.StreamDeck, new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), true);
            
            //GamingPanel
            Assert.True(AppEventHandler.OnProfileEventSubscribed());
            Assert.True(AppEventHandler.OnSavePanelSettingsSubscribed());
            Assert.True(AppEventHandler.OnSavePanelSettingsJSONSubscribed());

            //StreamDeckPanel
            Assert.True(SDEventHandler.OnStreamDeckSyncConfigurationEventSubscribed());
            Assert.True(SDEventHandler.OnDirtyConfigurationsEventHandlerEventSubscribed());
            Assert.True(SDEventHandler.OnDirtyNotificationEventHandlerSubscribed());
            Assert.True(SDEventHandler.OnStreamDeckShowNewLayerEventSubscribed());
            Assert.True(SDEventHandler.OnRemoteStreamDeckShowNewLayerEventSubscribed());
            Assert.True(SDEventHandler.OnStreamDeckSelectedButtonChangedEventSubscribed());
            Assert.True(SDEventHandler.OnStreamDeckClearSettingsEventSubscribed());
        }

        //[Fact]
        //public void Event_StreamDeck_SettingsModified_Proper_Detachment()
        //{
        //    var gamingPanelSkeleton =
        //        new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
        //    var streamDeckPanel = new StreamDeckPanel(GamingPanelEnum.StreamDeck, new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), true);
        //    streamDeckPanel.Dispose();

        //    //GamingPanel
        //    Assert.False(AppEventHandler.OnForwardPanelEventChangedSubscribed());
        //    Assert.False(AppEventHandler.OnProfileEventSubscribed());
        //    Assert.False(AppEventHandler.OnSavePanelSettingsSubscribed());
        //    Assert.False(AppEventHandler.OnSavePanelSettingsJSONSubscribed());

        //    //StreamDeckPanel
        //    Assert.False(SDEventHandler.OnStreamDeckSyncConfigurationEventSubscribed());
        //    Assert.False(SDEventHandler.OnDirtyConfigurationsEventHandlerEventSubscribed());
        //    Assert.False(SDEventHandler.OnDirtyNotificationEventHandlerSubscribed());
        //    Assert.False(SDEventHandler.OnStreamDeckShowNewLayerEventSubscribed());
        //    Assert.False(SDEventHandler.OnRemoteStreamDeckShowNewLayerEventSubscribed());
        //    Assert.False(SDEventHandler.OnStreamDeckSelectedButtonChangedEventSubscribed());
        //    Assert.False(SDEventHandler.OnStreamDeckClearSettingsEventSubscribed());
        //}
    }
}
