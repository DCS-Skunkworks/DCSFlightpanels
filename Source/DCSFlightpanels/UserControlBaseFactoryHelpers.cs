using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using NonVisuals.HID;
using System.Windows.Controls;
using DCSFlightpanels.PanelUserControls.CDU737;

namespace DCSFlightpanels
{
    internal static class UserControlBaseFactoryHelpers
    {
        public static IGamingPanelUserControl GetUSerControl(
            GamingPanelEnum gamingPanelType,
            DCSAircraft profile,
            HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            if (gamingPanelType == GamingPanelEnum.CDU737)
            {
                if (DCSAircraft.IsA10C(profile))
                {
                    return new Cdu737UserControlA10C(hidSkeleton);
                }
                if (DCSAircraft.IsAH64D(profile))
                {
                    return new Cdu737UserControlAH64D(hidSkeleton);
                }
                if (DCSAircraft.IsFA18C(profile))
                {
                    return new Cdu737UserControlFA18C(hidSkeleton);
                }
                if (DCSAircraft.IsSA342(profile))
                {
                    return new Cdu737UserControlSA342(hidSkeleton);
                }
                if (DCSAircraft.IsF14B(profile))
                {
                    return new Cdu737UserControlF14(hidSkeleton);
                }
                if (DCSAircraft.IsM2000C(profile))
                {
                    return new Cdu737UserControlM2000C(hidSkeleton);
                }
            }

            return null;

        }
    }
}