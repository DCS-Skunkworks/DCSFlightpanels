namespace NonVisuals.Saitek
{
    using System;
    using System.Collections.Generic;

    using NonVisuals.EventArgs;
    using NonVisuals.Saitek.Panels;

    public class BipEventHandlerManager
    {
        public delegate void BipPanelRegisteredEventHandler(object sender, BipPanelRegisteredEventArgs e);

        public event BipPanelRegisteredEventHandler OnBipPanelRegistered;

        private readonly List<BacklitPanelBIP> _backlitPanels = new List<BacklitPanelBIP>();

        // private object _panelLockObject = new object();
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
                if (bipLight.BindingHash == backlitPanelBIP.BindingHash)
                {
                    backlitPanelBIP.SetLED(bipLight.BIPLedPosition, bipLight.LEDColor);
                }
            }
        }

        public void LightUpGreen(string hash)
        {
            foreach (var backlitPanelBIP in _backlitPanels)
            {
                if (hash == backlitPanelBIP.BindingHash)
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
                if (hash == backlitPanelBIP.BindingHash)
                {
                    foreach (BIPLedPositionEnum position in Enum.GetValues(typeof(BIPLedPositionEnum)))
                    {
                        backlitPanelBIP.SetLED(position, PanelLEDColor.DARK);
                    }
                }
            }
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


        /*
         * Not sure about this one, I think it can be removed.
         */
        private void OnPanelRegistered(BacklitPanelBIP backlitPanelBip)
        {
            OnBipPanelRegistered?.Invoke(this, new BipPanelRegisteredEventArgs { HIDInstance = backlitPanelBip.HIDInstance, BacklitPanelBip = backlitPanelBip });
        }
    }
}
