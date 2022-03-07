using ClassLibraryCommon;
using DCS_BIOS.EventArgs;
using NonVisuals;
using NonVisuals.EventArgs;
using NonVisuals.Saitek.Panels;
using NonVisuals.StreamDeck.Events;
using NonVisuals.StreamDeck.Panels;
using Xunit;

namespace Tests.NonVisuals
{
    public class EventHandlerSubscriptionTests
    {
        /*************************************************************************************************
         *                                          SWITCHPANEL
         *************************************************************************************************/
        [Fact]
        public void Event_SwitchPanel_SettingsModified_Proper_Attachment()
        {
            AppEventHandler appEventHandler = new();
            GamingPanelSkeleton gamingPanelSkeleton = new (GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel);
            HIDSkeleton hIDSkeleton = new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", appEventHandler);
            var switchPanel = new SwitchPanelPZ55(hIDSkeleton);

            //SwitchPanel
            // Assert.True(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());

            //GamingPanel
            Assert.True(appEventHandler.OnForwardPanelEventChangedSubscribed());
            Assert.True(appEventHandler.OnProfileEventSubscribed());
            Assert.True(appEventHandler.OnSavePanelSettingsSubscribed());
            Assert.True(appEventHandler.OnSavePanelSettingsJSONSubscribed());
        }

        [Fact]
        public void Event_SwitchPanel_SettingsModified_Proper_Detachment()
        {
            AppEventHandler appEventHandler = new();
            GamingPanelSkeleton gamingPanelSkeleton = new(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ55SwitchPanel);
            HIDSkeleton hIDSkeleton = new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", appEventHandler);
            var switchPanel = new SwitchPanelPZ55(hIDSkeleton);
            switchPanel.Dispose();

            //SwitchPanel
            //Assert.False(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());

            //GamingPanel
            Assert.False(appEventHandler.OnForwardPanelEventChangedSubscribed());
            Assert.False(appEventHandler.OnProfileEventSubscribed());
            Assert.False(appEventHandler.OnSavePanelSettingsSubscribed());
            Assert.False(appEventHandler.OnSavePanelSettingsJSONSubscribed());
        }


        /*************************************************************************************************
         *                                          MULTIPANEL
         *************************************************************************************************/

        [Fact]
        public void Event_MultiPanel_SettingsModified_Proper_Attachment()
        {
            AppEventHandler appEventHandler = new();
            GamingPanelSkeleton gamingPanelSkeleton = new(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            HIDSkeleton hIDSkeleton = new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", appEventHandler);

            var multiPanelPZ70 = new MultiPanelPZ70(hIDSkeleton);

            //MultiPanel
            //Assert.True(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());

            //GamingPanel
            Assert.True(appEventHandler.OnForwardPanelEventChangedSubscribed());
            Assert.True(appEventHandler.OnProfileEventSubscribed());
            Assert.True(appEventHandler.OnSavePanelSettingsSubscribed());
            Assert.True(appEventHandler.OnSavePanelSettingsJSONSubscribed());
        }

        [Fact]
        public void Event_MultiPanel_SettingsModified_Proper_Detachment()
        {
            AppEventHandler appEventHandler = new();
            GamingPanelSkeleton gamingPanelSkeleton = new(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            HIDSkeleton hIDSkeleton = new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", appEventHandler);
            var multiPanelPZ70 = new MultiPanelPZ70(hIDSkeleton);
            multiPanelPZ70.Dispose();

            //MultiPanel
            //Assert.False(BIOSEventHandler.OnDcsDataAddressValueEventSubscribed());

            //GamingPanel
            Assert.False(appEventHandler.OnForwardPanelEventChangedSubscribed());
            Assert.False(appEventHandler.OnProfileEventSubscribed());
            Assert.False(appEventHandler.OnSavePanelSettingsSubscribed());
            Assert.False(appEventHandler.OnSavePanelSettingsJSONSubscribed());
        }


        /*************************************************************************************************
         *                                          STREAMDECKPANEL
         *************************************************************************************************/

        [Fact]
        public void Event_StreamDeck_OnDirtyConfigurations_Proper_Attachment()
        {
            AppEventHandler appEventHandler = new();
            GamingPanelSkeleton gamingPanelSkeleton = new(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            HIDSkeleton hIDSkeleton = new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", appEventHandler);
            var streamDeckPanel = new StreamDeckPanel(GamingPanelEnum.StreamDeck, hIDSkeleton, true);
            
            //GamingPanel
            Assert.True(appEventHandler.OnProfileEventSubscribed());
            Assert.True(appEventHandler.OnSavePanelSettingsSubscribed());
            Assert.True(appEventHandler.OnSavePanelSettingsJSONSubscribed());

            //StreamDeckPanel
            //Assert.True(SDEventHandler.OnStreamDeckSyncConfigurationEventSubscribed());
            //Assert.True(SDEventHandler.OnDirtyConfigurationsEventHandlerEventSubscribed());
            //Assert.True(SDEventHandler.OnDirtyNotificationEventHandlerSubscribed());
            //Assert.True(SDEventHandler.OnStreamDeckShowNewLayerEventSubscribed());
            //Assert.True(SDEventHandler.OnRemoteStreamDeckShowNewLayerEventSubscribed());
            //Assert.True(SDEventHandler.OnStreamDeckSelectedButtonChangedEventSubscribed());
            //Assert.True(SDEventHandler.OnStreamDeckClearSettingsEventSubscribed());
        }

        [Fact]
        public void Event_StreamDeck_SettingsModified_Proper_Detachment()
        {
            AppEventHandler appEventHandler = new();
            GamingPanelSkeleton gamingPanelSkeleton = new(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            HIDSkeleton hIDSkeleton = new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", appEventHandler);
            var streamDeckPanel = new StreamDeckPanel(GamingPanelEnum.StreamDeck, hIDSkeleton, true);
            streamDeckPanel.Dispose();

            //GamingPanel
            Assert.False(appEventHandler.OnForwardPanelEventChangedSubscribed());
            Assert.False(appEventHandler.OnProfileEventSubscribed());
            Assert.False(appEventHandler.OnSavePanelSettingsSubscribed());
            Assert.False(appEventHandler.OnSavePanelSettingsJSONSubscribed());

            //StreamDeckPanel
            //Assert.False(SDEventHandler.OnStreamDeckSyncConfigurationEventSubscribed());
            //Assert.False(SDEventHandler.OnDirtyConfigurationsEventHandlerEventSubscribed());
            //Assert.False(SDEventHandler.OnDirtyNotificationEventHandlerSubscribed());
            //Assert.False(SDEventHandler.OnStreamDeckShowNewLayerEventSubscribed());
            //Assert.False(SDEventHandler.OnRemoteStreamDeckShowNewLayerEventSubscribed());
            //Assert.False(SDEventHandler.OnStreamDeckSelectedButtonChangedEventSubscribed());
            //Assert.False(SDEventHandler.OnStreamDeckClearSettingsEventSubscribed());
        }
    }
}
