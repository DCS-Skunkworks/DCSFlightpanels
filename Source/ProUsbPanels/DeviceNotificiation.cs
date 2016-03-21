using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
//using LibUsbDotNet.DeviceNotify;
using Microsoft.Win32;

namespace ProUsbPanels
{
    public class DeviceNotificiation
    {
  //      private IDeviceNotifier _usbDeviceNotifier = null;
        private Thread _thread;
        private Exception _lastException;
        private bool _shutdown = false;

        delegate void CreateDeviceNotifierDelegate();

        private void CreateDeviceNotifier()
        {
             //_usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();
        }

        public void StartListener()
        {
            var createDeviceNotifierDelegate = new CreateDeviceNotifierDelegate(CreateDeviceNotifier);
            SystemEvents.InvokeOnEventsThread(createDeviceNotifierDelegate);
            _thread = new Thread(ListeningThread);
            _thread.Start();
        }

        public void StopListener()
        {
            _shutdown = true;
            Thread.Sleep(600);
            if(_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
            }
        }

        private void ListeningThread()
        {
            try
            {
                try
                {
                    // Hook the device notifier event
                    //_usbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;
                    // Exit on and key pressed.
                    Debug("Waiting for system level device events..");
                    while (!_shutdown)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,new Action(delegate { }));
                        Thread.Sleep(500);
                    }
                }
                finally
                {
                    //_usbDeviceNotifier.Enabled = false; // Disable the device notifier
                    // Unhook the device notifier event
                    // _usbDeviceNotifier.OnDeviceNotify -= OnDeviceNotifyEvent;
                }
            }
            catch (Exception ex)
            {
                _lastException = ex;
            }
        }
        /*
        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            // A Device system-level event has occured
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("\nProduct " + e.Device.IdProduct);
            stringBuilder.Append("\nVendor " + e.Device.IdVendor);
            stringBuilder.Append("\nName " + e.Device.Name);
            stringBuilder.Append("\nDevice Type " + e.DeviceType);
            stringBuilder.Append("\nEvent Type " + e.EventType);
            stringBuilder.Append("\n------------------------------------\n");
            Debug(e.ToString()); // Dump the event info to output.
        }
        */
        private void Debug(String str)
        {
            if (Common.Debug)
            {
                Console.WriteLine(str);
            }
        }
        /*
        public IDeviceNotifier UsbDeviceNotifier
        {
            get { return _usbDeviceNotifier; }
        }
        */
        public Exception LastException
        {
            get { return _lastException; }
        }
    }
}
