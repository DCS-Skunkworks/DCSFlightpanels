using System.Windows.Controls;
using DCSFlightpanels.Interfaces;
using NonVisuals.StreamDeck.Events;

namespace DCSFlightpanels
{
    public static class EventHandlers
    {
        public delegate void UserControlIsDirtyEventHandler(object sender, StreamDeckUIControlDirtyChangeArgs e);
        public static event UserControlIsDirtyEventHandler OnUserControlIsDirtyEventHandler;

        public static void AttachDirtyListener(IStreamDeckDirtyListener streamDeckDirtyListener)
        {
            OnUserControlIsDirtyEventHandler += streamDeckDirtyListener.IsDirtyControl;
        }

        public static void DetachDirtyListener(IStreamDeckDirtyListener streamDeckDirtyListener)
        {
            OnUserControlIsDirtyEventHandler -= streamDeckDirtyListener.IsDirtyControl;
        }

        public static void UserControlIsDirty(UserControl userControl)
        {
            var eventArguments = new StreamDeckUIControlDirtyChangeArgs() {UserControl = userControl};
            OnUserControlIsDirtyEventHandler?.Invoke(userControl, eventArguments);
        }
    }
}
