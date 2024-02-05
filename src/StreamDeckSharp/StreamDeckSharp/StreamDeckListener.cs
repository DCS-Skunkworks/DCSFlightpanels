using HidSharp;
using OpenMacroBoard.SDK;
using StreamDeckSharp.Internals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckSharp
{
    /// <summary>
    /// A listener for stream deck devices.
    /// </summary>
    public sealed class StreamDeckListener :
        IDisposable,
        IObservable<DeviceStateReport>
    {
        private readonly object _sync = new();
        private readonly List<DeviceState> _knownDevices = new();
        private readonly List<Subscription> _subscriptions = new();
        private readonly Dictionary<string, StreamDeckDeviceReference> _knownDeviceLookup = new();

        private bool _disposed = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDeckListener"/> class.
        /// </summary>
        public StreamDeckListener()
        {
            // register event handler before we load the entire list
            // so we don't miss stream decks connecting between the calls.
            DeviceList.Local.Changed += DeviceListChanged;

            // initial force load
            ProcessDelta();
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<DeviceStateReport> observer)
        {
            var subscription = new Subscription(this, observer);
            _subscriptions.Add(subscription);
            subscription.SendUpdates();
            return subscription;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DeviceList.Local.Changed -= DeviceListChanged;
        }

        private void ProcessDelta()
        {
            lock (_sync)
            {
                // because the HidDevice event doesn't tell us what changed
                // we calculate the difference ourselves.
                var currentDevices = DeviceList.Local
                    .GetStreamDecks()
                    .ToDictionary(s => s.DevicePath, s => s);

                // update connection states of known devices
                foreach (var knownDevice in _knownDevices)
                {
                    knownDevice.Connected = currentDevices.ContainsKey(knownDevice.DeviceReference.DevicePath);
                }

                // add new devices
                foreach (var currentDevice in currentDevices)
                {
                    if (_knownDeviceLookup.ContainsKey(currentDevice.Key))
                    {
                        // skip because this one is already known
                        continue;
                    }

                    _knownDeviceLookup.Add(currentDevice.Key, currentDevice.Value);
                    _knownDevices.Add(new DeviceState(currentDevice.Value, true));
                }

                // send updates to all subscribers
                foreach (var subscription in _subscriptions)
                {
                    subscription.SendUpdates();
                }
            }
        }

        private void DeviceListChanged(object sender, DeviceListChangedEventArgs e)
        {
            ProcessDelta();
        }

        private sealed class DeviceState
        {
            public DeviceState(StreamDeckDeviceReference deviceReference, bool connected)
            {
                DeviceReference = deviceReference ?? throw new ArgumentNullException(nameof(deviceReference));
                Connected = connected;
            }

            public StreamDeckDeviceReference DeviceReference { get; }
            public bool Connected { get; set; }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly StreamDeckListener _parent;
            private readonly IObserver<DeviceStateReport> _observer;

            /// <summary>
            /// Contains the state the subscriber knows about.
            /// This is used to calculate new updates.
            /// </summary>
            private readonly List<bool> _subscriberState = new();

            public Subscription(StreamDeckListener parent, IObserver<DeviceStateReport> observer)
            {
                _parent = parent ?? throw new ArgumentNullException(nameof(parent));
                _observer = observer ?? throw new ArgumentNullException(nameof(observer));
            }

            public void SendUpdates()
            {
                // send updates for existing devices
                for (int i = 0; i < _subscriberState.Count; i++)
                {
                    var device = _parent._knownDevices[i];

                    if (device.Connected != _subscriberState[i])
                    {
                        // report new connection state
                        _observer.OnNext(new DeviceStateReport(device.DeviceReference, device.Connected, false));
                        _subscriberState[i] = device.Connected;
                    }
                }

                // add and send updates for new (to this subscriber) devices.
                for (int i = _subscriberState.Count; i < _parent._knownDevices.Count; i++)
                {
                    var device = _parent._knownDevices[i];
                    _subscriberState.Add(device.Connected);
                    _observer.OnNext(new DeviceStateReport(device.DeviceReference, device.Connected, true));
                }
            }

            public void Dispose()
            {
                lock (_parent._sync)
                {
                    _parent._subscriptions.Remove(this);
                }
            }
        }
    }
}
