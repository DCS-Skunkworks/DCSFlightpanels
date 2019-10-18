using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace NonVisuals.Saitek
{
    public static class BipFactory
    {
        private static readonly BipEventHandlerManager BIPEventHandlerManager = new BipEventHandlerManager();

        public static void BroadcastRegisteredBips()
        {
            BIPEventHandlerManager.BroadcastRegisteredBips();
        }

        public static bool HasBips()
        {
            return BIPEventHandlerManager.GetBips().Count > 0;
        }

        public static void RegisterBip(BacklitPanelBIP backlitPanelBip)
        {
            BIPEventHandlerManager.RegisterBip(backlitPanelBip);
        }

        public static void DeRegisterBip(BacklitPanelBIP backlitPanelBip)
        {
            BIPEventHandlerManager.DeRegisterBip(backlitPanelBip);
        }

        public static List<BacklitPanelBIP> GetBips()
        {
            return BIPEventHandlerManager.GetBips();
        }

        public static BipEventHandlerManager GetBipEventHandlerManager()
        {
            if (!HasBips())
            {
                return null;
            }
            return BIPEventHandlerManager;
        }

        public static void AddBipListener(IGamingPanelListener iGamingPanelListener)
        {
            BIPEventHandlerManager.AddBipListener(iGamingPanelListener);
        }

        public static void RemoveBipListener(IGamingPanelListener iGamingPanelListener)
        {
            BIPEventHandlerManager.RemoveBipListener(iGamingPanelListener);
        }

        public static void LightUpGreen(string hash)
        {
            BIPEventHandlerManager.LightUpGreen(hash);
        }

        public static void SetDark(string hash)
        {
            BIPEventHandlerManager.SetDark(hash);
        }

        public static void SetAllDark()
        {
            BIPEventHandlerManager.SetAllDark();
        }

        public static void ShowLight(BIPLight bipLight)
        {
            BIPEventHandlerManager.ShowLight(bipLight);
        }
    }

    public class BipEventHandlerManager
    {

        public delegate void BipPanelRegisteredEventHandler(object sender, BipPanelRegisteredEventArgs e);
        public event BipPanelRegisteredEventHandler OnBipPanelRegistered;

        private readonly List<BacklitPanelBIP> _backlitPanels = new List<BacklitPanelBIP>();
        //private object _panelLockObject = new object();
        
        public List<BacklitPanelBIP> GetBips()
        {
            return _backlitPanels;
        }

        public void SetAllDark()
        {
            foreach (var backlitPanelBIP in _backlitPanels)
            {
                foreach (BIPLedPositionEnum position in Enum.GetValues(typeof(BIPLedPositionEnum)))
                {
                    backlitPanelBIP.SetLED(position, PanelLEDColor.DARK);
                }
            }
        }

        public void ShowLight(BIPLight bipLight)
        {
            foreach (var backlitPanelBIP in _backlitPanels)
            {
                if (bipLight.Hash == backlitPanelBIP.Hash)
                {
                    backlitPanelBIP.SetLED(bipLight.BIPLedPosition, bipLight.LEDColor);
                }
            }
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

        public void AddBipListener(IGamingPanelListener iGamingPanelListener)
        {
            OnBipPanelRegistered += iGamingPanelListener.BipPanelRegisterEvent;
        }

        public void RemoveBipListener(IGamingPanelListener iGamingPanelListener)
        {
            OnBipPanelRegistered -= iGamingPanelListener.BipPanelRegisterEvent;
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
            OnBipPanelRegistered?.Invoke(this, new BipPanelRegisteredEventArgs() { UniqueId = backlitPanelBip.InstanceId, BacklitPanelBip = backlitPanelBip });
        }
    }

    public class BipPanelRegisteredEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public BacklitPanelBIP BacklitPanelBip { get; set; }
    }
}
