using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    public interface ISettingsModifiedListener
    {
        /*
         * Used by ProfileHandler to detect changes in panel configurations.
         */
        void SettingsModified(object sender, PanelEventArgs e); 
    }
}
