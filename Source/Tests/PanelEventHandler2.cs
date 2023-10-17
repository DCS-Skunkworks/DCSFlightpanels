using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using MEF;

namespace DCSFPTests
{
    /*
     * Use this class as a template for your plugin.
     * Reference the MEF project where the interface and necessary files are located.
     */
    [Export(typeof(IPanelEventHandler))]
    [ExportMetadata("Name", "Sample Plugin 2 - MessageBox events to user")]
    public class PanelEventHandler2 : IPanelEventHandler
    {
        private string PluginName { get {
                return (string)GetType()
                    .GetCustomAttributes(false)
                    .OfType<ExportMetadataAttribute>()
                    .Single(attribute => attribute.Name == "Name").Value;
            }
        }

        public void PanelEvent(string profile, string panelHidId, PluginGamingPanelEnum panel, int switchId, bool pressed, SortedList<int, IKeyPressInfo> keySequence)
        {
            /*
             * Your code here
             */
            MessageBox.Show($"Profile : {profile}\nPanelID : {panel}\nSwitchId : {switchId}\nAction : {(pressed ? "Pressed" : "Released")}", $"This is a message from {PluginName}");
        }

        public void Settings()
        {
            MessageBox.Show($"This would be the settings for plugin {PluginName}", "You clicked the plugin menu.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
