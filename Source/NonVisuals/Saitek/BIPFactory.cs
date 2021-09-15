namespace NonVisuals.Saitek
{
    using System.Collections.Generic;

    using NonVisuals.Interfaces;
    using NonVisuals.Saitek.Panels;

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

        public static void AddBipListener(IGamingPanelListener gamingPanelListener)
        {
            BIPEventHandlerManager.AddBipListener(gamingPanelListener);
        }

        public static void RemoveBipListener(IGamingPanelListener gamingPanelListener)
        {
            BIPEventHandlerManager.RemoveBipListener(gamingPanelListener);
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
