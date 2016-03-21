using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace NonVisuals
{

    /*
     * 
     * 1) Initialize DirectOutput
     * 2) Register Callback for attached and detached FIPs
     * 3) Enumerate all attached FIPs, create new FIPPanel objects and register callbacks
     * 4)   
     *      
     */
    public class FIPHandler
    {
        public delegate void FIPCountHasChangedEventHandler(int numberFIPsConnected);
        public event FIPCountHasChangedEventHandler OnFIPCountChanged;

        private List<FIPPanel> _fipPanels = new List<FIPPanel>();
        private bool _initOk;
        private DirectOutputClass.DeviceCallback _deviceCallback;
        private DirectOutputClass.EnumerateCallback _enumerateCallback;

        public bool Initialize()
        {
            _initOk = false;
            try
            {
                _deviceCallback = DeviceCallback;
                _enumerateCallback = EnumerateCallback;

                if (!File.Exists(@"C:\Program Files\Saitek\DirectOutput\DirectOutput.dll"))
                {
                    //Did not find Saitek drivers
                    Common.LogError(1, "FIPHandler failed to init. No Saitek drivers found.");
                    return false;
                }

                var retVal = DirectOutputClass.Initialize("ABC");
                if (retVal != ReturnValues.S_OK)
                {
                    Common.LogError(1, "FIPHandler failed to init DirectOutputClass. retval = " + retVal);
                    return false;
                }
                Debug.Print("Init returned: " + retVal);

                retVal = DirectOutputClass.RegisterDeviceCallback(_deviceCallback);
                Debug.Print("Register Device Callback returned: " + retVal);

                retVal = DirectOutputClass.Enumerate(_enumerateCallback);
                if (retVal != ReturnValues.S_OK)
                {
                    Common.LogError(1, "FIPHandler failed to Enumerate DirectOutputClass. retval = " + retVal);
                    return false;
                }
                Debug.Print("Enumerate Callback returned: " + retVal);
            }
            catch (Exception ex)
            {
                Common.LogError(1, ex, "FIPHandler failed to init.");
                return false;
            }
            _initOk = true;
            return true;
        }

        public bool InitOk
        {
            get { return _initOk; }
        }

        public void Close()
        {
            try
            {
                foreach (var fipPanel in _fipPanels)
                {
                    fipPanel.Shutdown();
                }
                var retVal = DirectOutputClass.Deinitialize();
                //Console.WriteLine(retVal);
            }
            catch (Exception e)
            {
                Common.LogError(234323, e);
            }
        }

        ~FIPHandler()
        {
            try
            {/*
                foreach (var fipPanel in _fipPanels)
                {
                    fipPanel.Close();
                }*/
                DirectOutputClass.Deinitialize();
            }
            catch (Exception)
            {
            }
        }

        private void DeviceCallback(IntPtr device, bool added, IntPtr context)
        {
            //Called whenever a DirectOutput device is added or removed from the system.
            Debug.Print("DeviceCallback(): 0x" + device.ToString("x") + (added ? " Added" : " Removed"));
            if (GetDeviceType(device) != DeviceTypes.Fip)
            {
                return;
            }
                if (!added && _fipPanels.Count == 0)
            {
                return;
            }

            var i = _fipPanels.Count - 1;
            var found = false;
            do
            {
                if (_fipPanels[i].DevicePtr == device)
                {
                    found = true;
                    var fip = _fipPanels[i];
                    if (!added)
                    {
                        fip.Shutdown();
                        _fipPanels.Remove(fip);
                    }
                }
                i--;
            } while (i >= 0);
            if (added && !found)
            {
                Debug.Print("DeviceCallback() Spawning one FIPPanelA10C. " + device);
                var fipPanel = new FIPPanelA10C(device, this);
                fipPanel.FIPHandler = this;
                _fipPanels.Add(fipPanel);
                fipPanel.Initalize();
            }
            if (OnFIPCountChanged != null)
            {
                OnFIPCountChanged(_fipPanels.Count);
            }
        }

        private DeviceTypes GetDeviceType(IntPtr device)
        {
            Guid m_guid = Guid.Empty;
            DeviceTypes retVal = DeviceTypes.X52Pro;

            DirectOutputClass.GetDeviceType(device, ref m_guid);

            if (string.Compare(m_guid.ToString(), DirectOutputClass.DeviceTypeFip, true, CultureInfo.InvariantCulture) == 0)
            {
                retVal = DeviceTypes.Fip;
            }

            return retVal;
        }

        private void EnumerateCallback(IntPtr device, IntPtr context)
        {
            //Called initially when enumerating FIPs.
            try
            {
                if (GetDeviceType(device) != DeviceTypes.Fip)
                {
                    return;
                }
                Debug.Print("Spawning one [FIPPanelA10C]. EnumerateCallback() Device: 0x" + device.ToString("x"));
                var fipPanel = new FIPPanelA10C(device, this);
                fipPanel.FIPHandler = this;
                _fipPanels.Add(fipPanel);
                fipPanel.Initalize();
            }
            catch (Exception ex)
            {
                Common.LogError(788788, ex, "EnumerateCallback for FIP " + device);
                throw;
            }
        }


        public void IterateForClosedFIPs()
        {
            try
            {
                _fipPanels.RemoveAll(x => x.Closed == true);
            }
            catch (Exception ex)
            {
                Common.LogError(783988, ex, "IterateForClosedFIPs ");
                throw;
            }
        }

        public List<FIPPanel> FIPPanels
        {
            get { return _fipPanels; }
        }
    }

}
