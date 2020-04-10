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
