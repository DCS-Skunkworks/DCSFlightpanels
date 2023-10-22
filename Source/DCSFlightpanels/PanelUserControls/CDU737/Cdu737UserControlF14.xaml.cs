namespace DCSFlightpanels.PanelUserControls.PreProgrammed
{
    using System.Windows;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;

    using NonVisuals.CockpitMaster.Preprogrammed;
    using Interfaces;
    using System;
    using NonVisuals.Panels;
    using NonVisuals.HID;

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

        private bool _once = true;

        private void CDU737UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            UserControlLoaded = true;

            if (_once)
            {
                _once = false;
            }
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

