using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using DCSFlightpanels.PanelUserControls.PreProgrammed;
using NonVisuals.HID;
using System.Windows.Controls;

namespace DCSFlightpanels
{
    internal static class UserControlBaseFactoryHelpers
    {
        public static IGamingPanelUserControl GetUSerControl(
            GamingPanelEnum gamingPanelType,
            DCSFPProfile profile,
            HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            if (gamingPanelType == GamingPanelEnum.CDU737)
            {
                if (DCSFPProfile.IsA10C(profile))
                {
                    return new Cdu737UserControlA10C(hidSkeleton);
                }
                if (DCSFPProfile.IsAH64D(profile))
                {
                    return new Cdu737UserControlAH64D(hidSkeleton);
                }
                if (DCSFPProfile.IsFA18C(profile))
                {
                    return new Cdu737UserControlFA18C(hidSkeleton);
                }
                if (DCSFPProfile.IsSA342(profile))
                {
                    return new Cdu737UserControlSA342(hidSkeleton);
                }
                if (DCSFPProfile.IsF14B(profile))
                {
                    return new Cdu737UserControlF14(hidSkeleton);
                }
                if (DCSFPProfile.IsM2000C(profile))
                {
                    return new Cdu737UserControlM2000C(hidSkeleton);
                }
            }

            return null;

        }
    }
}