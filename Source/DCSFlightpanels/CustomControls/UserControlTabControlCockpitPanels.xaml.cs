using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using DCSFlightpanels.PanelUserControls;
using DCSFlightpanels.Radios.Emulators;
using DCSFlightpanels.Radios.PreProgrammed;
using NonVisuals.EventArgs;
using NonVisuals.HID;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DCSFlightpanels.CustomControls
{
    /// <summary>
    /// Interaction logic for UserControlTabControlCockpitPanels.xaml
    /// </summary>
    public partial class UserControlTabControlCockpitPanels : UserControl
    {
        public int SelectedIndex
        {
            get { return TabControlPanels.SelectedIndex;}
            set { TabControlPanels.SelectedIndex = value;}
        }

        public UserControlTabControlCockpitPanels()
        {
            InitializeComponent();
        }


        public void AddPanel(HIDSkeleton hidSkeleton, DCSAircraft dcsAircraft, ref List<KeyValuePair<string, GamingPanelEnum>> profileFileHIDInstances)
        {
            try
            {
                if (!hidSkeleton.IsAttached)
                {
                    return;
                }

                
                switch (hidSkeleton.GamingPanelType)
                {
                    case GamingPanelEnum.CDU737:
                        {
                            var tabItem = new TabItem { Header = "CDU 737" };

                            IGamingPanelUserControl panel = UserControlBaseFactoryHelpers.GetUSerControl(GamingPanelEnum.CDU737,
                                                dcsAircraft,
                                                hidSkeleton, tabItem);
                            if (panel != null)
                            {
                                tabItem.Content = panel;
                                TabControlPanels.Items.Add(tabItem);

                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));
                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }

                            break;


                        }
                    case GamingPanelEnum.PZ55SwitchPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ55" };
                            var switchPanelPZ55UserControl = new SwitchPanelPZ55UserControl(hidSkeleton);
                            tabItem.Content = switchPanelPZ55UserControl;
                            TabControlPanels.Items.Add(tabItem);
                            profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.PZ70MultiPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ70" };
                            var multiPanelUserControl = new MultiPanelUserControl(hidSkeleton);
                            tabItem.Content = multiPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.BackLitPanel:
                        {
                            var tabItem = new TabItem { Header = "B.I.P." };
                            var backLitPanelUserControl = new BackLitPanelUserControl(hidSkeleton);
                            tabItem.Content = backLitPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.TPM:
                        {
                            var tabItem = new TabItem { Header = "TPM" };
                            var tpmPanelUserControl = new TPMPanelUserControl(hidSkeleton);
                            tabItem.Content = tpmPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.StreamDeckMini:
                    case GamingPanelEnum.StreamDeckMiniV2:
                    case GamingPanelEnum.StreamDeck:
                    case GamingPanelEnum.StreamDeckV2:
                    case GamingPanelEnum.StreamDeckMK2:
                    case GamingPanelEnum.StreamDeckXL:
                    case GamingPanelEnum.StreamDeckXLRev2:
                    case GamingPanelEnum.StreamDeckPlus:
                        {
                            var tabItemStreamDeck = new TabItem { Header = hidSkeleton.GamingPanelType.GetEnumDescriptionField() };
                            var streamDeckUserControl = new StreamDeckUserControl(hidSkeleton.GamingPanelType, hidSkeleton);
                            tabItemStreamDeck.Content = streamDeckUserControl;
                            TabControlPanels.Items.Add(tabItemStreamDeck);
                            profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);

                            break;
                        }

                    case GamingPanelEnum.FarmingPanel:
                        {
                            var tabItem = new TabItem { Header = "Side Panel" };
                            var farmingSidePanelUserControl = new FarmingPanelUserControl(hidSkeleton);
                            tabItem.Content = farmingSidePanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.PZ69RadioPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ69" };
                            if (DCSAircraft.IsKeyEmulator(dcsAircraft))
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlEmulator(hidSkeleton);
                                    tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (Common.IsEmulationModesFlagSet(EmulationMode.SRSEnabled) || DCSAircraft.IsFlamingCliff(dcsAircraft))
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSRS(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsA10C(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                // True means A-10C II
                                UserControlBase radioPanelPZ69UserControl = dcsAircraft.Option1 ? new RadioPanelPZ69UserControlA10CII(hidSkeleton) : new RadioPanelPZ69UserControlA10C(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsUH1H(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlUH1H(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMiG21Bis(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMiG21Bis(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsKa50(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlKa50(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMi8MT(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi8(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsBf109K4(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlBf109(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsFW190D9(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFw190(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsP51D(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP51D(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF86F(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF86F(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsSpitfireLFMkIX(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSpitfireLFMkIX(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsAJS37(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAJS37(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsSA342(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSA342(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsFA18C(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFA18C(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsM2000C(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlM2000C(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF5E(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF5E(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF14B(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF14B(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsAV8B(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAV8BNA(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsP47D(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP47D(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsT45C(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlT45C(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMi24P(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi24P(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsAH64D(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAH64D(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF16C(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF16C(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMosquito(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMosquito(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsYak52(dcsAircraft) && !dcsAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlYak52(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlGeneric(hidSkeleton);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                /*
                                 * If the module doesn't have a pre-programmed radio it will end up here. If this is a new user profile
                                 * then set the value here so that if there is a pre-programmed radio available in the future it won't cause
                                 * problems. The problem would be that when the user loads the profile the pre-programmed radio is loaded
                                 * but the user has configs for the generic radio.
                                 * I.e. no pre-programmed radio exists => UseGenericRadio = true.
                                 */
                                dcsAircraft.UseGenericRadio = true;
                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }

                            break;
                        }
                }

                SortTabs();

                if (TabControlPanels.Items.Count > 0)
                {
                    TabControlPanels.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SortTabs()
        {
            var panelOrderList = new List<GamingPanelEnum>
            {
                GamingPanelEnum.StreamDeckPlus,
                GamingPanelEnum.CDU737,
                GamingPanelEnum.StreamDeckXL,
                GamingPanelEnum.StreamDeck,
                GamingPanelEnum.StreamDeckV2,
                GamingPanelEnum.StreamDeckMK2,
                GamingPanelEnum.StreamDeckMini,
                GamingPanelEnum.StreamDeckMiniV2,
                GamingPanelEnum.BackLitPanel,
                GamingPanelEnum.PZ69RadioPanel,
                GamingPanelEnum.TPM,
                GamingPanelEnum.PZ70MultiPanel,
                GamingPanelEnum.PZ55SwitchPanel
            };

            foreach (var gamingPanelEnum in panelOrderList)
            {
                for (var i = 0; i < TabControlPanels.Items.Count; i++)
                {
                    var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                    var userControl = (IGamingPanelUserControl)tabItem.Content;

                    var panelType = userControl.GetPanelType();
                    if (panelType == gamingPanelEnum)
                    {
                        TabControlPanels.Items.RemoveAt(i);
                        TabControlPanels.Items.Insert(0, tabItem);
                    }
                }
            }

            TabControlPanels.SelectedIndex = 0;
        }
        
        public void DisposePanel(HIDSkeleton hidSkeleton)
        {
            void Action()
            {
                for (var i = 0; i < TabControlPanels.Items.Count; i++)
                {
                    var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                    var userControl = (IGamingPanelUserControl)tabItem.Content;

                    if (userControl.GetGamingPanel().HIDInstance.Equals(hidSkeleton.HIDInstance))
                    {
                        userControl.Dispose();
                        TabControlPanels.Items.RemoveAt(i);
                        AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Disposed);
                        break;
                    }
                }
            }

            Dispatcher?.BeginInvoke((Action)Action);
        }

        public int DisposePanels()
        {
            var closedItemCount = 0;
            try
            {
                if (TabControlPanels.Items.Count > 0)
                {
                    do
                    {
                        var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(0);
                        var userControl = (UserControlBase)tabItem.Content;
                        TabControlPanels.Items.Remove(tabItem);
                        var gamingPanelUserControl = (IGamingPanelUserControl)tabItem.Content;
                        var gamingPanel = gamingPanelUserControl.GetGamingPanel();

                        if (gamingPanel != null)
                        {
                            userControl.Dispose();
                            closedItemCount++;
                        }
                    }
                    while (TabControlPanels.Items.Count > 0);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

            return closedItemCount;
        }
    }
}
