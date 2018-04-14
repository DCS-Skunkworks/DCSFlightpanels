using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public static class BipFactory
    {
        private static BipEventHandlerManager _bipEventHandlerManager = new BipEventHandlerManager();

        public static void BroadcastRegisteredBips()
        {
            _bipEventHandlerManager.BroadcastRegisteredBips();
        }

        public static bool HasBips()
        {
            return _bipEventHandlerManager.GetBips().Count > 0;
        }

        public static void RegisterBip(BacklitPanelBIP backlitPanelBip)
        {
            _bipEventHandlerManager.RegisterBip(backlitPanelBip);
        }

        public static void DeRegisterBip(BacklitPanelBIP backlitPanelBip)
        {
            _bipEventHandlerManager.DeRegisterBip(backlitPanelBip);
        }

        public static List<BacklitPanelBIP> GetBips()
        {
            return _bipEventHandlerManager.GetBips();
        }
        
        public static void AddBipListener(ISaitekPanelListener iSaitekPanelListener)
        {
            _bipEventHandlerManager.AddBipListener(iSaitekPanelListener);
        }
        
        public static void RemoveBipListener(ISaitekPanelListener iSaitekPanelListener)
        {
            _bipEventHandlerManager.RemoveBipListener(iSaitekPanelListener);
        }

        public static void LightUpGreen(string hash)
        {
            _bipEventHandlerManager.LightUpGreen(hash);
        }

        public static void SetDark(string hash)
        {
            _bipEventHandlerManager.SetDark(hash);
        }
    }

    public class BipEventHandlerManager
    {

        public delegate void BipPanelRegisteredEventHandler(object sender, BipPanelRegisteredEventArgs e);
        public event BipPanelRegisteredEventHandler OnBipPanelRegistered;

        private List<BacklitPanelBIP> _backlitPanels = new List<BacklitPanelBIP>();

        public List<BacklitPanelBIP> GetBips()
        {
            return _backlitPanels;
        }

        public void LightUpGreen(string hash)
        {
            foreach (var backlitPanelBIP in _backlitPanels)
            {
                if (hash == backlitPanelBIP.Hash)
                {
                    foreach (BIPLedPositionEnum position in Enum.GetValues(typeof(BIPLedPositionEnum)))
                    {
                        backlitPanelBIP.SetLED(position, PanelLEDColor.GREEN);
                    }
                }
            }
        }

        public void SetDark(string hash)
        {
            foreach (var backlitPanelBIP in _backlitPanels)
            {
                if (hash == backlitPanelBIP.Hash)
                {
                    foreach (BIPLedPositionEnum position in Enum.GetValues(typeof(BIPLedPositionEnum)))
                    {
                        backlitPanelBIP.SetLED(position, PanelLEDColor.DARK);
                    }
                }
            }
        }

        public void AddBipListener(ISaitekPanelListener iSaitekPanelListener)
        {
            OnBipPanelRegistered += iSaitekPanelListener.BipPanelRegisterEvent;
        }

        public void RemoveBipListener(ISaitekPanelListener iSaitekPanelListener)
        {
            OnBipPanelRegistered -= iSaitekPanelListener.BipPanelRegisterEvent;
        }

        public void BroadcastRegisteredBips()
        {
            foreach (var backlitPanelBip in _backlitPanels)
            {
                OnPanelRegistered(backlitPanelBip);
            }
        }

        public void RegisterBip(BacklitPanelBIP backlitPanelBip)
        {
            _backlitPanels.Add(backlitPanelBip);
            OnPanelRegistered(backlitPanelBip);
        }

        public void DeRegisterBip(BacklitPanelBIP backlitPanelBip)
        {
            _backlitPanels.Remove(backlitPanelBip);
        }

        private void OnPanelRegistered(BacklitPanelBIP backlitPanelBip)
        {
            if (OnBipPanelRegistered != null)
            {
                OnBipPanelRegistered(this, new BipPanelRegisteredEventArgs() { UniqueId = backlitPanelBip.InstanceId, BacklitPanelBip =  backlitPanelBip});
            }
        }
    }

    public class BipPanelRegisteredEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public BacklitPanelBIP BacklitPanelBip { get; set; }
    }
}
