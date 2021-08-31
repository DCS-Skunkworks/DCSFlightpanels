using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.Plugin
{
    interface IPanelEventPlugin
    {
        string PluginName { get; set; }
        /*
         * eventType : 0 Released / Inactive, 1 Pressed / Active
         * All id's for panels and switches can be found in Switched.cs
         */
        void PanelEvent(int panelId, int switchId, int eventType);
    }
}
