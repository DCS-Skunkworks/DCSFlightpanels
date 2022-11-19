namespace NonVisuals.Panels.Saitek
{
    using System.Collections.Generic;
    using Panels;
    using Panels;

    /*
     * This class is used to manage all the BIPs that are connected to the PC.
     * When the user makes e.g. a BipLink it uses this class to see which BIPs are
     * available and can identify which is which by having the BIP flash its LEDs.
     */
    public static class BipFactory
    {
        private static readonly BipEventHandlerManager BIPEventHandlerManager = new();

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
}
