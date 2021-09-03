namespace SamplePanelEventPlugin
{
    using System;
    using System.ComponentModel.Composition;

    [Export(typeof(IPanelEventHandler))]
    public class PanelEventHandler : IPanelEventHandler
    {
        public string PluginGuid =>
            /*
             * Generate a new Guid for YOUR plugin
             */
            "b2d3e289-6388-4d42-9256-861acf0990e7";

        public string PluginName => "Sample Plugin";

        public void PanelEvent(string profile, string panelHidId, int panelId, int switchId, int eventType, int extraInfo)
        {
            /*
             * Your code here
             */
            PanelEventFileWriter.WriteInfo(profile, panelHidId, panelId, switchId, eventType, extraInfo);
        }
    }
}
