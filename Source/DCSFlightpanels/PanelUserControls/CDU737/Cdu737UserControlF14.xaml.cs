using System;
using System.Windows;
using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using NonVisuals.CockpitMaster.PreProgrammed;
using NonVisuals.EventArgs;
using NonVisuals.HID;
using NonVisuals.Interfaces;
using NonVisuals.Panels;

namespace DCSFlightpanels.PanelUserControls.CDU737
{
    /// <summary>
    /// Interaction logic for Cdu737UserControlF14.xaml
    /// </summary>
    /// 
    public partial class Cdu737UserControlF14 : IGamingPanelListener,
        IGamingPanelUserControl
    {
        private readonly CDU737PanelF14 _cdu737PanelF14;

        public Cdu737UserControlF14(HIDSkeleton hidSkeleton)
        {
            InitializeComponent();

            _cdu737PanelF14 = new CDU737PanelF14(hidSkeleton);
            
            //_HIDSkeleton = hidSkeleton;
            AppEventHandler.AttachGamingPanelListener(this);

            HideAllImages();

        }
        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cdu737PanelF14.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);

                }
                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        public override void Init()
        {
            try
            {
                _cdu737PanelF14.InitPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CDU737UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            UserControlLoaded = true;
        }

        public override GamingPanel GetGamingPanel()
        {
            return _cdu737PanelF14;
        }


        public void ProfileEvent(object sender, ProfileEventArgs e)
        {

        }

        public void SettingsApplied(object sender, PanelInfoArgs e)
        {

        }

        public void SettingsModified(object sender, PanelInfoArgs e)
        {

        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            string[] lines = _cdu737PanelF14.CDULines;
            Dispatcher?.BeginInvoke(
            (Action)
            (() =>
            {
                CDU737UserControl.DisplayLines(lines, 10);
            }
            ));
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {

        }
        private static void HideAllImages()
        {

        }

        public string GetName()
        {
            return GetType().Name;
        }

    }
}

