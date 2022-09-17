using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using DCSFlightpanels.PanelUserControls.PreProgrammed;
using NonVisuals;
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
                    return new Cdu737UserControlA10C(hidSkeleton, parentTabItem);

                }
                if (DCSFPProfile.IsAH64D(profile))
                {
                    return new Cdu737UserControlAH64D(hidSkeleton, parentTabItem);
                }
                if (DCSFPProfile.IsFA18C(profile))
                {
                    return new Cdu737UserControlFA18C(hidSkeleton, parentTabItem);
                }

            }

            return null;

        }
    }
}