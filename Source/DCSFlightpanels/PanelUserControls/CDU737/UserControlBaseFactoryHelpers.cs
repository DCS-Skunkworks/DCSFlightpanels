using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;
using NonVisuals.HID;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DCSFlightpanels.PanelUserControls.CDU737
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
                    var control = new Cdu737UserControlA10C(hidSkeleton);
                    control.Init();
                    return control;
                }
                if (DCSAircraft.IsAH64D(profile))
                {
                    var control = new Cdu737UserControlAH64D(hidSkeleton);
                    control.Init();
                    return control;
                }
                if (DCSAircraft.IsFA18C(profile))
                {
                    var control = new Cdu737UserControlFA18C(hidSkeleton);
                    control.Init();
                    return control;
                }
                if (DCSAircraft.IsSA342(profile))
                {
                    var control = new Cdu737UserControlSA342(hidSkeleton);
                    control.Init();
                    return control;
                }
                if (DCSAircraft.IsF14B(profile))
                {
                    var control = new Cdu737UserControlF14(hidSkeleton);
                    control.Init();
                    return control;
                }
                if (DCSAircraft.IsM2000C(profile))
                {
                    var control = new Cdu737UserControlM2000C(hidSkeleton);
                    control.Init();
                    return control;
                }
            }

            return null;

        }
    }
}