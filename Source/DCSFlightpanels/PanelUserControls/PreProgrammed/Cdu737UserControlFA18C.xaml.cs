namespace DCSFlightpanels.PanelUserControls.PreProgrammed
{
    using System.Windows;
    using System.Windows.Controls;
    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;

    using NonVisuals.CockpitMaster.Preprogrammed;
    using Interfaces;
    using System;

    /// <summary>
    /// Interaction logic for Cdu737UserControlA10C.xaml
    /// </summary>
    /// 
    public partial class Cdu737UserControlFA18C : UserControlBase,
        IGamingPanelListener,
        IGamingPanelUserControl
    {
        private readonly CDU737PanelFA18C _CDU737PanelFA18C;

        public Cdu737UserControlFA18C(HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            InitializeComponent();
            ParentTabItem = parentTabItem;

            _CDU737PanelFA18C = new CDU737PanelFA18C(hidSkeleton);
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
                    _CDU737PanelFA18C.Dispose();
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
            return _CDU737PanelFA18C;
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
            string[] lines = _CDU737PanelFA18C.CDULines;
            Dispatcher?.BeginInvoke(
            (Action)
            (() =>
            {
                CDU737UserControl.displayLines(lines, 10);
            }
            ));
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {

        }
        private void HideAllImages()
        {

        }

        public string GetName()
        {
            return GetType().Name;
        }

    }
}

