using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals;

namespace DCSFlightpanels.Event_Propagators
{
    public abstract class SettingsChangedPropagator
    {
        public delegate void SettingsHasChangedEventHandler(object sender, PanelEventArgs e);
        public event SettingsHasChangedEventHandler OnSettingsChangedA;


        protected virtual void SettingsChanged()
        {
            //OnSettingsChangedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }
    }
}
