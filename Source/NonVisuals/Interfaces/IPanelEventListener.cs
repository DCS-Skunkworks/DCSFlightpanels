using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    public interface IPanelEventListener
    {
        /*
         * Used for announcing panel events, attached, removed, found.
         */
        public void PanelEvent(object sender, PanelEventArgs e);
    }
}
