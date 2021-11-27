

// Resharper Disable all
namespace DCSFlightpanels.Radios.PreProgrammed
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    using ClassLibraryCommon;

    using DCSFlightpanels.Interfaces;
    using DCSFlightpanels.PanelUserControls;

    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Saitek;

    /// <summary>
    /// Interaction logic for RadioPanelPZ69UserControlNotImplemented.xaml
    /// </summary>
    public partial class RadioPanelPZ69UserControlNotImplemented : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl
    {
        public RadioPanelPZ69UserControlNotImplemented(HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            InitializeComponent();
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
        }

        public override GamingPanel GetGamingPanel()
        {
            return null;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.PZ69RadioPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public void ProfileSelected(object sender, AirframeEventArgs e) { }

        public void UISwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                SetGraphicsState(e.Switches);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e){}

        public void SettingsCleared(object sender, PanelEventArgs e) { }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e) { }
        
        public void DeviceAttached(object sender, PanelEventArgs e) { }

        public void SettingsApplied(object sender, PanelEventArgs e) { }

        public void PanelSettingsModified(object sender, PanelEventArgs e) { }

        public void DeviceDetached(object sender, PanelEventArgs e) { }

        private void SetGraphicsState(HashSet<object> knobs)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }
        
        private void HideAllImages()
        {
        }

        


        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void RadioPanelPZ69UserControlNotImplemented_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
